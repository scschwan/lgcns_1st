using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceTool
{
    public class userControlHandler
    {
        public static uc_preprocessing uc_Preprocessing = new uc_preprocessing();
        public static uc_FileLoad uc_fileLoad = new uc_FileLoad();
        public static uc_Classification uc_classification = new uc_Classification();
        public static uc_DataTransform uc_dataTransform = new uc_DataTransform();
        public static uc_Clustering uc_clustering = new uc_Clustering();
    }
}
