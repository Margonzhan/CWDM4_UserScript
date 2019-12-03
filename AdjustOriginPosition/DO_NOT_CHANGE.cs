using System;
using System.Linq;
using HalconDotNet;
using System.Drawing;
using System.IO;
using AdjustOriginPosition.Service;
using AdjustOriginPosition.CamRAC;
namespace AdjustOriginPosition
{
    /// <summary>
    /// =========================== ATTENTION ===========================
    /// ===========================    注意   =========================== 
    /// =                                                               =  
    /// =          Please DO NOT make ANY changes to this file.         =
    /// =                    请勿修改当前文件的任何内容。                   =
    /// =                                                               =
    /// =================================================================
    /// 
    /// </summary>
    partial class APAS_UserScript
    {
        static void Main(string[] args)
        {
            CamRemoteAccessContractClient camClient = new CamRemoteAccessContractClient();

            var client = new SystemServiceClient();

            try
            {
                client.Open();

                client.__SSC_Connect();

                // perform the user process.
                BenchMarkSet(client, camClient);

                client.__SSC_Disonnect();
            }
            catch (AggregateException ae)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Red;

                var ex = ae.Flatten();

                ex.InnerExceptions.ToList().ForEach(e =>
                {
                    Console.WriteLine($"Error occurred, {e.Message}");
                });

                Console.ResetColor();
            }
            finally
            {
                client.Close();
            }
            //Console.WriteLine("Press any key to exit.");

            //Console.ReadKey();
        }
    }
}
