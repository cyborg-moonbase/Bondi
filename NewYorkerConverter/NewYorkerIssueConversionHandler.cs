using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using NLog;


namespace NewYorkerConverter
{
    public class NewYorkerIssueConversionHandler
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        public void Handle(string issueFilePath, string outputFolderPath)
        {

            if (string.IsNullOrWhiteSpace(issueFilePath))
            {
                //error 
            }

            string fileName = Path.GetFileNameWithoutExtension(issueFilePath);

            string issueYear = fileName.Split('_')[0];
            string issueDate = fileName.Replace("_", "");
            
            string issueOutputFolderPath = Path.Combine(outputFolderPath,issueYear,issueDate,"Pages");
            string thumbnailOutputFolderPath = Path.Combine(outputFolderPath,  issueYear, issueDate, "Thumbnails");

            if (!Directory.Exists(issueOutputFolderPath))
            {
                logger.Info("Creating folder for pages of issue {0} at {1}", issueDate, issueOutputFolderPath);
                Directory.CreateDirectory(issueOutputFolderPath);
            }

            if (!Directory.Exists(thumbnailOutputFolderPath))
            {
                logger.Info("Creating folder for thumbnails of issue {0} at {1}", issueDate, thumbnailOutputFolderPath);
                Directory.CreateDirectory(thumbnailOutputFolderPath);
            }



            FMManagedLoader fMManagedLoader = new FMManagedLoader(issueFilePath);
            uint pageCount = fMManagedLoader.PageCount();

            logger.Info("Starting to process issue {0}.", issueDate);
            logger.Info("Issue {0} has {1} pages.", issueDate, pageCount);

            try {
                for (uint i = 0; i < pageCount; i++)
                {
                    logger.Info("Starting to process page {0} of issue {1}", i+1, issueDate);


                    var height = fMManagedLoader.HeightForPage(i);
                    var width = fMManagedLoader.WidthForPage(i) ;
                    var gamma = fMManagedLoader.GammaForPage(i);

                    logger.Info("Issue:{0} Page:{1} Height:{2} Width:{3} Gamma:{4}", issueDate, i + 1, height, width, gamma);


                    Bitmap pageBitmap = new Bitmap((int)width, (int)height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    fMManagedLoader.RenderBitmap(pageBitmap, i, gamma);

                    //Create thumbnail with is 10% of the size of the original. 
                    Bitmap thumbnailBitmap =  new Bitmap(pageBitmap, (int)width / 10, (int)height / 10);
 

                    var pageFileName = string.Format("{0}.jpeg", (i + 1));

                    var pageOutputFilePath = Path.Combine(issueOutputFolderPath, pageFileName);
                    var thumbnailOutputFilePath = Path.Combine(thumbnailOutputFolderPath, pageFileName);


                    logger.Info("Issue:{0} Page:{1} OutputFilePath:{2}", issueDate, i + 1, pageOutputFilePath);
                    
                    pageBitmap.Save(pageOutputFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    thumbnailBitmap.Save(thumbnailOutputFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                     

                    GC.Collect();
                }
                logger.Info("Completed processing issue {0}", fileName);
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Exception occured while processing issue {0}", fileName);
                logger.Info("Removing folder for issue");
                Directory.Delete(issueOutputFolderPath, true); 
            }
             
            fMManagedLoader.__dtor();
            GC.Collect();
        }



    }
}
