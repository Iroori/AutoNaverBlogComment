using RJBlogProject.Service;
using System;


namespace RJBlogProject
{
    class Program
    {
        
        [STAThread]
        static void Main(string[] args)
        {
            string blogId = "rcolfree";

            var naverService = new NaverLoginService();
            naverService.OpenWebMode();
            naverService.Login("", "");
            naverService.GoToBlog(blogId);

            var blogService = new NaverBlogCommentService(naverService.Driver);
            blogService.GoToLatestPost(blogId);
            blogService.ReplyToLatestComment();

            naverService.CloseBrowser();
        }
    }
}
