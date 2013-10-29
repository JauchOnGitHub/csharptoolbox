using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDF5DotNet;

namespace HDF5Test
{
   class Program
   {
      static void Main(string[] args)
      {
         H5FileId file_id;         
         float[,] data = new float[718, 359];
         H5Array<float> h5_data = new H5Array<float>(data);

         try
         {
            file_id = H5F.open("gfs.hdf5", H5F.OpenMode.ACC_RDONLY);

            if (file_id.Id >= 0)
            {
               H5GroupId i_gid = H5G.open(file_id, "/Results/wind modulus");                  

               for (int i = 0; i < 200000; i++)
               {                  
                  H5DataSetId i_dsid = H5D.open(i_gid, "wind modulus_00001");
                  H5DataTypeId i_dt = H5D.getType(i_dsid);
                  H5DataTypeId i_ndt = H5T.getNativeType(i_dt, H5T.Direction.DESCEND);                  
                  H5D.read(i_dsid, i_ndt, h5_data);
                  H5T.close(i_ndt);
                  H5T.close(i_dt);
                  H5D.close(i_dsid);                  
               }

               H5G.close(i_gid);
            }


            H5F.close(file_id);
         }
         catch (Exception ex)
         {
            Console.WriteLine("An exception happened. The message returned was:");
            Console.WriteLine("{0}", ex.Message);
            Console.ReadKey();
         }
      }
   }
}
