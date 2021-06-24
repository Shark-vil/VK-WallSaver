using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VK_WallSaver.Extensions;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;

namespace VK_WallSaver
{
    class Program
    {
        internal static string ApplicationPath = AppContext.BaseDirectory;
        internal static VkApi api = new VkApi();

        private static string StoragePath = Path.Combine(ApplicationPath, "DownloadedFiles");
        private static int CurrentAsynchronous = 0;
        private static int LimitAsynchronous = 5;

        static void Main(string[] args) => Menu();

        private static void Menu()
        {
            bool HasCloasedApplication = false;
            bool HasLogged = false;

            while (!HasCloasedApplication)
            {
                Console.Clear();

                if (HasLogged)
                {
                    EConsole.Print("1 - Group selection");
                    EConsole.Print("2 - User selection");
                    EConsole.Print("0 - Close application");

                    string Key = EConsole.ReadKey();

                    Console.Clear();

                    try
                    {
                        switch (Key)
                        {
                            case "1":
                                GroupSelection();
                                break;

                            case "2":
                                UserSelection();
                                break;

                            case "0":
                                HasCloasedApplication = true;
                                break;
                        }
                    }
                    catch(Exception ex)
                    {
                        EConsole.Warning("An uncommitted exception was thrown:");
                        EConsole.Error(ex);
                        EConsole.ClickToContinue();
                    }
                }
                else
                {
                    EConsole.Print("1 - Authorization");
                    EConsole.Print("0 - Close application");

                    string Key = EConsole.ReadKey();

                    Console.Clear();

                    try
                    {
                        switch (Key)
                        {
                            case "1":
                                HasLogged = Login();
                                break;

                            case "0":
                                HasCloasedApplication = true;
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        EConsole.Warning("An uncommitted exception was thrown:");
                        EConsole.Error(ex);
                        EConsole.ClickToContinue();
                    }
                }
            }
        }

        private static bool Login()
        {
            if (Authorizer.Default(api))
                return true;
            else
            {
                EConsole.Warning("Failed to sign in to your account :(");
                EConsole.ClickToContinue();
            }

            return false;
        }

        private static string GetNameOfUrl()
        {
            EConsole.Print("Enter page url:");
            string PageUniqueId = EConsole.ReadKey();
            PageUniqueId = PageUniqueId.Replace("https://", string.Empty);
            PageUniqueId = PageUniqueId.Replace("http://", string.Empty);
            PageUniqueId = PageUniqueId.Replace("vk.com/", string.Empty);
            PageUniqueId = PageUniqueId.Replace("m.vk.com/", string.Empty);
            return PageUniqueId;
        }

        private static void GroupSelection()
        {
            string PageUniqueId = GetNameOfUrl();

            ReadOnlyCollection<Group> groups = api.Groups.GetById(new string[] { }, PageUniqueId, GroupsFields.Description);
            Group group = groups.FirstOrDefault();
            if (group == null)
                return;

            Downloader(-group.Id);       
        }

        private static void UserSelection()
        {
            string PageUniqueId = GetNameOfUrl();

            ReadOnlyCollection<User> users = api.Users.Get(new string[] { PageUniqueId });
            User user = users.FirstOrDefault();
            if (user == null)
                return;

            Downloader(user.Id);
        }

        private static void Downloader(long OwnerId)
        {
            EConsole.Print("Enter any archive name:");
            string ArchiveName = EConsole.ReadKey();
            ArchiveName = (ArchiveName.Length == 0) ? OwnerId.ToString() : ArchiveName;

            if (!Directory.Exists(StoragePath))
                Directory.CreateDirectory(StoragePath);

            string LocalStoragePath = Path.Combine(StoragePath, ArchiveName);

            if (!Directory.Exists(LocalStoragePath))
                Directory.CreateDirectory(LocalStoragePath);

            var client = new WebClient();
            var VisualProgressBar = new ProgressBar();

            WallGetObject Posts;
            ulong PostsOffset = 0;
            ulong TotalCount = 0;
            ulong ProgressCount = 0;

            do
            {
                Posts = api.Wall.Get(new WallGetParams
                {
                    OwnerId = OwnerId,
                    Count = 50,
                    Offset = PostsOffset,
                });

                if (TotalCount == 0)
                    TotalCount = Posts.TotalCount;

                foreach (Post post in Posts.WallPosts)
                {
                    foreach (Attachment PostAttachment in post.Attachments)
                    {
                        if (PostAttachment.Type == typeof(Photo))
                        {
                            var photo = (Photo)PostAttachment.Instance;
                            ulong lastWidth = 0;
                            ulong lastHeight = 0;
                            PhotoSize finalPhoto = null;

                            foreach (PhotoSize pSize in photo.Sizes)
                            {
                                if (pSize.Width > lastWidth && pSize.Height > lastHeight)
                                {
                                    lastWidth = pSize.Width;
                                    lastHeight = pSize.Height;
                                    finalPhoto = pSize;
                                }
                            }

                            if (finalPhoto != null && finalPhoto.Url != null)
                            {
                                string FileName = Path.GetFileName(finalPhoto.Url.LocalPath);
                                string SavedPath = Path.Combine(LocalStoragePath, FileName);

                                if (!File.Exists(SavedPath))
                                {
                                    if (CurrentAsynchronous < LimitAsynchronous && (ProgressCount / TotalCount) < 90)
                                    {
                                        CurrentAsynchronous++;

                                        var AsyncClient = new WebClient();
                                        AsyncClient.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
                                        {
                                            if (CurrentAsynchronous - 1 >= 0)
                                                CurrentAsynchronous--;

                                            VisualProgressBar.SetProgressText($"{FileName} ( {ProgressCount}/{TotalCount} )");
                                        };
                                        AsyncClient.DownloadFileAsync(finalPhoto.Url, SavedPath);
                                    }
                                    else
                                    {
                                        client.DownloadFile(finalPhoto.Url, SavedPath);
                                        VisualProgressBar.SetProgressText($"{FileName} ( {ProgressCount}/{TotalCount} )");
                                    }
                                }
                            }
                        }
                    }

                    ProgressCount++;
                    VisualProgressBar.Report((double)ProgressCount / (double)TotalCount);
                    Thread.Sleep(50);
                }

                PostsOffset += 100;
                Thread.Sleep(1000);
            }
            while (Posts.WallPosts.Count != 0);

            VisualProgressBar.Dispose();

            EConsole.ClickToContinue();
        }
    }
}
