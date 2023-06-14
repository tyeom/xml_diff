using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GitDiff_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private string GetRelativePath(string basePath, string fullPath)
        {
            Uri baseUri = new Uri(basePath);
            Uri fullUri = new Uri(fullPath);

            Uri relativeUri = baseUri.MakeRelativeUri(fullUri);

            return Uri.UnescapeDataString(relativeUri.ToString());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Git repository 경로 설정
            string repositoryPath = @"C:\Users\banaple_43\Documents\카카오톡 받은 파일";

            // Git repository 열기
            using (var repo = new Repository(repositoryPath))
            {
                // 파일 경로를 Git 상대 경로로 변환
                string relativeFilePath1 = GetRelativePath(repositoryPath, "file11.xml");
                string relativeFilePath2 = GetRelativePath(repositoryPath, "file12.xml");

                // 첫 번째 파일의 변경 내용 가져오기
                var commit1 = repo.Head.Tip;
                var treeEntry1 = commit1[relativeFilePath1];
                var blob1 = (Blob)treeEntry1.Target;
                string content1 = blob1.GetContentText();

                // 두 번째 파일의 변경 내용 가져오기
                var commit2 = repo.Head.Tip;
                var treeEntry2 = commit2[relativeFilePath2];
                var blob2 = (Blob)treeEntry2.Target;
                string content2 = blob2.GetContentText();

                // 변경 내용 출력
                Console.WriteLine("Changes in XML file 1:");
                Console.WriteLine(content1);

                Console.WriteLine("Changes in XML file 2:");
                Console.WriteLine(content2);
            }
        }
    }
}
