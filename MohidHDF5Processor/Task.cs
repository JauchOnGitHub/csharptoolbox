using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid;
using Mohid.Core;
using Mohid.Configuration;
using Mohid.Files;
using HDF5DotNet;

namespace MohidHDF5Processor
{
   public class HDFOutputObjectInfo
   {
      public string InputName, InputName2,
                    Name, Path;
      
      public H5GType Type;
      public bool IsProperty;
      public TaskPropertyInfo PropertyInfo;
      public HDFObjectInfo Input, Input2;

      public HDFOutputObjectInfo Parent;
      public List<HDFOutputObjectInfo> Children;      

      public HDFOutputObjectInfo()
      {
         Children = new List<HDFOutputObjectInfo>(); 
         Name = "";
         InputName = "";
         InputName2 = "";
         IsProperty = false;
         Type = H5GType.GROUP;
         PropertyInfo = null;
         Path = "/";
      }

   }

   public struct TaskGlobalOptions
   {
      public string DateTimeFormat;

      public FileName Input,
                      Output;

      public bool UseMapping;
      public string Mapping;

      //public bool DefineGridGroup,
      //            DefineTimeGroup,
      //            DefineResultsGroup;

      //public bool UseStart,
      //            UseEnd;
      //public DateTime Start,
      //                End;      
   }

   public class TaskGroupInfo
   {
      public string OutputPath,
                    InputPath;
      public H5GroupId OutputID,
                       InputID;

      public bool SaveAll;
   }

   public class TaskPropertyInfo
   {
      public string OutputName,
                    OutputSubGroup,
                    OutputPath,
                    InputName,                    
                    InputPath,
                    InputName2,                    
                    InputPath2;

      public bool UseComposition,
                  UseMultFactor,
                  UseValueToAdd,
                  IsWind10To2,
                  ImposeMin,
                  ImposeMax,
                  DoShiftHoriz,
                  DoShiftVert,
                  InvertHoriz,
                  InvertVert;

      public double MultFactor,
                    ValueToAdd,
                    MinValue,
                    MaxValue;

      public int ShiftHoriz,
                 ShiftVert;

      public string Units;
      public bool IgnoreMapping;
   }

   public class TaskEngine
   {
      protected string[] separator = {"/"};
      protected List<string> not_to_save;
      protected HDFObjectInfo input_root;
      protected HDFOutputObjectInfo output_root;
      protected Exception last_exception;
      protected List<TaskGroupInfo> group_list;
      protected List<TaskPropertyInfo> property_list;
      protected HDFEngine input,
                          output;
      protected int[] Mapping1D; 
      protected int[,] Mapping2D;
      protected int[,,] Mapping3D;
      public TaskGlobalOptions Options;

      protected object data, data2, temp;
      int last_NDims = -1;
      long[] last_Dims = null, last_MaxDims = null;
      H5DataTypeId last_DTId = null, last_NativeDTId = null;
      H5T.H5TClass lastClass = H5T.H5TClass.INTEGER;
      int last_Size = -1;

      H5Array<int> H5Aint, H5Aint2;
      H5Array<float> H5Afloat, H5Afloat2;
      H5Array<double> H5Adouble, H5Adouble2;
      Dictionary<string, KeywordData> Units;
    

      public Exception LastException
      {
         get
         {
            Exception to_return = last_exception;
            last_exception = null;
            return to_return;
         }
      }

      public TaskEngine()
      {
         last_exception = null;

         Options.DateTimeFormat = "dd/MM/yyyy";
         //Options.DefineGridGroup = true;
         //Options.DefineResultsGroup = true;
         //Options.DefineTimeGroup = true;
         Options.Input = null;
         Options.Output = null;
         //Options.UseStart = false;
         //Options.UseEnd = false;
         //Options.Start = DateTime.Now;
         //Options.End = DateTime.Now;

         Units = new Dictionary<string, KeywordData>();

         group_list = new List<TaskGroupInfo>();
         property_list = new List<TaskPropertyInfo>();

         input = new HDFEngine();
         output = new HDFEngine();

         output_root = new HDFOutputObjectInfo();
         input_root = new HDFObjectInfo();
         not_to_save = new List<string>();

         data = null;
         data2 = null;
         temp = null;
      }

      public void End()
      {
         input.CloseHDF();
         output.CloseHDF();
         output.CloseLibrary();
      }

      public bool LoadConfig(ConfigNode cfg)
      {
         try
         {
            LoadGlobalConfig(cfg);
            ReadInputHDFStructure();
            if (Options.UseMapping)
               LoadMapping();
            LoadGroupsToSave(cfg);
            LoadExceptions(cfg);
            //CheckGroupsInfo();
            LoadUnitsList(cfg);
            LoadProcessingInfo(cfg);
            //LinkProperties();
            CreateOutputStructure();


            last_exception = null;
            return true;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return false;
         }
      }

      protected void LoadUnitsList(ConfigNode cfg)
      {
         try
         {
            ConfigNode units_block = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "units"; });
            if (units_block != null)
            {
               foreach (KeyValuePair<string, KeywordData> pair in units_block.NodeData)
               {
                  Units[pair.Key] = pair.Value;
               }
            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return;
         }
      }

      protected void LoadMapping()
      {
         HDFObjectInfo obj = FindItem(Options.Mapping, input_root.Children);

         if (obj.Type == H5GType.GROUP)
         {
            if (obj.IsProperty)
            {
               throw new Exception("The mapping must be a DATASET, but was provided a GROUP");
            }
            else
               throw new Exception("The mapping '" + Options.Mapping + "' is invalid.");
         }
         else
         {
            H5GroupId gid = H5G.open(input.FileID, obj.Parent.Path);
            H5DataSetId did = H5D.open(gid, obj.Name);

            switch(obj.NDims)
            {
               case 1:
                  Mapping1D = new int[obj.Dims[0]];
                  H5D.read(did,obj.NativeType,new H5Array<int>(Mapping1D));      
                  break;
               case 2:
                  Mapping2D = new int[obj.Dims[0], obj.Dims[1]];
                  H5D.read(did,obj.NativeType,new H5Array<int>(Mapping2D));
                  break;
               case 3:
                  Mapping3D = new int[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                  H5D.read(did,obj.NativeType,new H5Array<int>(Mapping3D));
                  break;
            }

            H5D.close(did);
            H5G.close(gid);
         }         
      }

      public bool CreateNewHDF()
      {
         try
         {
            CreateHDF();

            H5GroupId id = H5G.open(output.FileID, "/");
            if (id.Id < 0)
               throw new Exception();
            
            WriteStructure(output_root, id);

            H5G.close(id);

            //WriteGroups();
            //WriteProperties();

            last_exception = null;
            return true;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return false;
         }
      }

      protected void WriteAttributes(H5DataSetId o_dsid, float d_min, float d_max, string units)
      {
         H5DataSpaceId ds_id = H5S.create(H5S.H5SClass.SCALAR);
         H5DataTypeId type_id = H5T.copy(H5T.H5Type.NATIVE_FLOAT); 
         H5DataTypeId native_id = H5T.getNativeType(type_id, H5T.Direction.DESCEND);
         H5AttributeId min_id = H5A.create(o_dsid, "Minimum", native_id, ds_id);
         H5AttributeId max_id = H5A.create(o_dsid, "Maximum", native_id, ds_id);

         float [] data = new float[1];
         H5Array<float> h5_array = new H5Array<float>(data);

         data[0] = d_min;
         H5A.write(min_id, native_id, h5_array);

         data[0] = d_max;
         H5A.write(max_id, native_id, h5_array);

         H5DataTypeId s_t_id = H5T.copy(H5T.H5Type.C_S1);
         
         byte[] str = System.Text.Encoding.ASCII.GetBytes(units);
         H5T.setSize(s_t_id, str.Length);

         H5AttributeId units_id = H5A.create(o_dsid, "Units", s_t_id, ds_id);
         H5A.write<byte>(units_id, s_t_id, new H5Array<byte>(str));

         h5_array = null;
         
         H5A.close(units_id);
         H5T.close(s_t_id);
         H5A.close(min_id);
         H5T.close(native_id);
         H5T.close(type_id);
         H5S.close(ds_id);
      }

      protected void WriteAttributes(H5GroupId o_dsid, float d_min, float d_max)
      {
         H5DataSpaceId ds_id = H5S.create(H5S.H5SClass.SCALAR);
         H5DataTypeId type_id = H5T.copy(H5T.H5Type.NATIVE_FLOAT);
         H5DataTypeId native_id = H5T.getNativeType(type_id, H5T.Direction.DESCEND);
         H5AttributeId min_id = H5A.create(o_dsid, "Minimum", native_id, ds_id);
         H5AttributeId max_id = H5A.create(o_dsid, "Maximum", native_id, ds_id);

         float[] data = new float[1];
         H5Array<float> h5_array = new H5Array<float>(data);

         data[0] = d_min;
         H5A.write(min_id, native_id, h5_array);

         data[0] = d_max;
         H5A.write(max_id, native_id, h5_array);

         h5_array = null;

         H5A.close(min_id);
         H5T.close(native_id);
         H5T.close(type_id);
         H5S.close(ds_id);
      }

      protected void WriteStructure(HDFOutputObjectInfo item, H5LocId id)
      {         
         int i, j, k;
         float g_min = 9.9E15f, g_max = -9.9E15f, d_min, d_max;
         string units;
         bool use_mapping;

         if (item.IsProperty || (item.Input != null && item.Input.HasDatasets))
         {
            foreach (HDFObjectInfo obj in item.Input.Children)
            {
               if (obj.Type == H5GType.DATASET)
               {
                  d_min = 9.9E15f;
                  d_max = -9.9E15f;

                  H5DataSpaceId o_sid = H5S.create_simple(obj.NDims, obj.Dims);
                  H5PropertyListId o_pid = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
                  H5PropertyListId o_pid_acc = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
                  H5PropertyListId o_pid_l = H5P.create(H5P.PropertyListClass.LINK_CREATE);
                  H5P.setChunk(o_pid, obj.Dims);
                  H5P.setDeflate(o_pid, 6);
                  H5GroupId o_gid = H5G.open(output.FileID, item.Path);
                  H5DataSetId o_dsid;

                  string name;
                  if (item.PropertyInfo == null || !item.PropertyInfo.UseComposition)
                     name = obj.Name;
                  else
                  {
                     name = item.Name + "_" + (obj.Index + 1).ToString("D5");
                  }

                  o_dsid = H5D.create(o_gid, name, obj.NativeType, o_sid, o_pid_l, o_pid, o_pid_acc);

                  H5GroupId i_gid = H5G.open(input.FileID, item.Input.Path);
                  //1. Open the dataset
                  H5DataSetId i_dsid = H5D.open(i_gid, obj.Name);

                  //check dimensions
                  bool different_dims = false;
                  if (last_Dims == null)
                     different_dims = true;
                  else
                     switch (obj.NDims)
                     {
                        case 1:
                           if (last_Dims[0] != obj.Dims[0])
                              different_dims = true;
                           break;
                        case 2:
                           if (last_Dims[0] != obj.Dims[0] ||
                               last_Dims[1] != obj.Dims[1])
                              different_dims = true;
                           break;
                        case 3:
                           if (last_Dims[0] != obj.Dims[0] ||
                               last_Dims[1] != obj.Dims[1] ||
                               last_Dims[2] != obj.Dims[2])
                              different_dims = true;
                           break;
                     }

                  //2. Allocate space for the data.
                  if (different_dims  || data == null ||
                      last_NDims != obj.NDims ||                      
                      lastClass != obj.Class ||
                      last_Size != obj.Size)
                  {
                     last_NDims = obj.NDims;
                     last_Dims = obj.Dims;
                     last_MaxDims = obj.MaxDims;
                     last_DTId = obj.DataTypeId;
                     last_NativeDTId = obj.NativeType;
                     lastClass = obj.Class;
                     last_Size = obj.Size;

                     data = null;
                     data2 = null;
                     H5Afloat = null;
                     H5Afloat2 = null;
                     H5Adouble = null;
                     H5Adouble2 = null;
                     H5Aint = null;
                     H5Aint2 = null;

                     switch (obj.NDims)
                     {
                        case 1:
                           switch (obj.Class)
                           {
                              case H5T.H5TClass.FLOAT:
                                 if (obj.Size == 4)
                                 {
                                    data = (float[])new float[obj.Dims[0]];                                    
                                    data2 = (float[])new float[obj.Dims[0]];
                                    H5Afloat = new H5Array<float>((float[])data);
                                    H5Afloat2 = new H5Array<float>((float[])data2);
                                 }
                                 else if (obj.Size == 8)
                                 {
                                    data = (double[])new double[obj.Dims[0]];
                                    data2 = (double[])new double[obj.Dims[0]];
                                    H5Adouble = new H5Array<double>((double[])data);
                                    H5Adouble2 = new H5Array<double>((double[])data2);
                                 }
                                 break;
                              case H5T.H5TClass.INTEGER:
                                 data = (int[])new int[obj.Dims[0]];
                                 data2 = (int[])new int[obj.Dims[0]];
                                 H5Aint = new H5Array<int>((int[])data);
                                 H5Aint2 = new H5Array<int>((int[])data2);
                                 break;
                           }
                           break;
                        case 2:
                           switch (obj.Class)
                           {
                              case H5T.H5TClass.FLOAT:
                                 if (obj.Size == 4)
                                 {
                                    data = (float[,])new float[obj.Dims[0], obj.Dims[1]];
                                    data2 = (float[,])new float[obj.Dims[0], obj.Dims[1]];
                                    H5Afloat = new H5Array<float>((float[,])data);
                                    H5Afloat2 = new H5Array<float>((float[,])data2);
                                 }
                                 else if (obj.Size == 8)
                                 {
                                    data = (double[,])new double[obj.Dims[0], obj.Dims[1]];
                                    data2 = (double[,])new double[obj.Dims[0], obj.Dims[1]];
                                    H5Adouble = new H5Array<double>((double[,])data);
                                    H5Adouble2 = new H5Array<double>((double[,])data2);
                                 }
                                 break;
                              case H5T.H5TClass.INTEGER:
                                 data = (int[,])new int[obj.Dims[0], obj.Dims[1]];
                                 data2 = (int[,])new int[obj.Dims[0], obj.Dims[1]];
                                 H5Aint = new H5Array<int>((int[,])data);
                                 H5Aint2 = new H5Array<int>((int[,])data2);
                                 break;
                           }
                           break;
                        case 3:
                           switch (obj.Class)
                           {
                              case H5T.H5TClass.FLOAT:
                                 if (obj.Size == 4)
                                 {
                                    data = (float[, ,])new float[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                    data2 = (float[, ,])new float[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                 }
                                 else if (obj.Size == 8)
                                 {
                                    data = (double[, ,])new double[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                    data2 = (double[, ,])new double[obj.Dims[0], obj.Dims[1], obj.Dims[2]];                                    
                                 }
                                 break;
                              case H5T.H5TClass.INTEGER:
                                 data = (int[, ,])new int[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                 data2 = (int[, ,])new int[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                 break;
                           }
                           break;
                     }
                  }
                  switch (obj.NDims)
                  {
                     case 1:
                        switch (obj.Class)
                        {
                           case H5T.H5TClass.FLOAT:
                              if (obj.Size == 4)
                              {                                    
                                 H5D.read(i_dsid, obj.NativeType, H5Afloat);
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, H5Afloat);
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                    units = item.PropertyInfo.Units;
                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       //float[] data2 = new float[obj.Dims[0]];
                                       H5D.read(i_dsid_2, obj.NativeType, H5Afloat2);

                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((float[])data)[i] = (float) Math.Sqrt(Math.Pow(((float[])data)[i], 2) + Math.Pow(((float[])data2)[i], 2));
                                       }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);
                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((float[])data)[i] = ((float[])data)[i] * (float)4.87 / ((float)Math.Log(67.8 * 10.0 + 5.42));
                                       }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((float[])data)[i] = ((float[])data)[i] * (float)item.PropertyInfo.MultFactor;
                                       }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((float[])data)[i] = ((float[])data)[i] + (float)item.PropertyInfo.ValueToAdd;
                                       }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             if (((float[])data)[i] > (float)item.PropertyInfo.MaxValue)
                                                ((float[])data)[i] = (float)item.PropertyInfo.MaxValue;
                                       }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             if (((float[])data)[i] < (float)item.PropertyInfo.MinValue)
                                                ((float[])data)[i] = (float)item.PropertyInfo.MinValue;
                                       }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    H5D.write(o_dsid, obj.NativeType, H5Afloat);
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                 {
                                    if (!use_mapping || (Mapping1D[i] == 1))
                                    {
                                       if (((float[])data)[i] <= d_min)
                                          d_min = ((float[])data)[i];
                                       if (((float[])data)[i] >= d_max)
                                          d_max = ((float[])data)[i];
                                       if (((float[])data)[i] <= g_min)
                                          g_min = ((float[])data)[i];
                                       if (((float[])data)[i] >= g_max)
                                          g_max = ((float[])data)[i];
                                    }
                                 }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              else if (obj.Size == 8)
                              {
                                 //data = (double[])new double[obj.Dims[0]];
                                 H5D.read(i_dsid, obj.NativeType, H5Adouble);
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, H5Adouble);
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                    units = item.PropertyInfo.Units;
                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       //double[] data2 = new double[obj.Dims[0]];
                                       H5D.read(i_dsid_2, obj.NativeType, H5Adouble2);

                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((double[])data)[i] = (double)Math.Sqrt(Math.Pow(((double[])data)[i], 2) + Math.Pow(((double[])data2)[i], 2));
                                       }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);
                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((double[])data)[i] = ((double[])data)[i] * (double)4.87 / ((double)Math.Log(67.8 * 10.0 + 5.42));
                                       }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((double[])data)[i] = ((double[])data)[i] * (double)item.PropertyInfo.MultFactor;
                                       }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             ((double[])data)[i] = ((double[])data)[i] + (double)item.PropertyInfo.ValueToAdd;
                                       }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             if (((double[])data)[i] > (double)item.PropertyInfo.MaxValue)
                                                ((double[])data)[i] = (double)item.PropertyInfo.MaxValue;
                                       }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                       {
                                          if (!use_mapping || (Mapping1D[i] == 1))
                                             if (((double[])data)[i] < (double)item.PropertyInfo.MinValue)
                                                ((double[])data)[i] = (double)item.PropertyInfo.MinValue;
                                       }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    H5D.write(o_dsid, obj.NativeType, H5Adouble);
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                 {
                                    if (!use_mapping || (Mapping1D[i] == 1))
                                    {
                                       if ((float)((double[])data)[i] <= d_min)
                                          d_min = (float)((double[])data)[i];
                                       if ((float)((double[])data)[i] >= d_max)
                                          d_max = (float)((double[])data)[i];
                                       if ((float)((double[])data)[i] <= g_min)
                                          g_min = (float)((double[])data)[i];
                                       if ((float)((double[])data)[i] >= g_max)
                                          g_max = (float)((double[])data)[i];
                                    }
                                 }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              break;
                           case H5T.H5TClass.INTEGER:
                              //data = (int[])new int[obj.Dims[0]];
                              H5D.read(i_dsid, obj.NativeType, H5Aint);
                              if (item.PropertyInfo == null)
                              {
                                 H5D.write(o_dsid, obj.NativeType, H5Aint);
                                 units = "-";
                                 use_mapping = false;
                              }
                              else
                              {
                                 use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                 units = item.PropertyInfo.Units;
                                 if (item.PropertyInfo.UseComposition)
                                 {
                                    H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                    //1. Open the dataset
                                    string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                    H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                    //int[] data2 = new int[obj.Dims[0]];
                                    H5D.read(i_dsid_2, obj.NativeType, H5Aint2);

                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          ((int[])data)[i] = (int)Math.Sqrt(Math.Pow(((int[])data)[i], 2) + Math.Pow(((int[])data2)[i], 2));
                                    }

                                    H5D.close(i_dsid_2);
                                    H5G.close(i_gid_2);
                                 }

                                 if (item.PropertyInfo.IsWind10To2)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          ((int[])data)[i] = ((int[])data)[i] * (int)4.87 / ((int)Math.Log(67.8 * 10.0 + 5.42));
                                    }
                                 }

                                 if (item.PropertyInfo.UseMultFactor)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          ((int[])data)[i] = ((int[])data)[i] * (int)item.PropertyInfo.MultFactor;
                                    }
                                 }

                                 if (item.PropertyInfo.UseValueToAdd)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          ((int[])data)[i] = ((int[])data)[i] + (int)item.PropertyInfo.ValueToAdd;
                                    }
                                 }

                                 if (item.PropertyInfo.ImposeMax)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          if (((int[])data)[i] > (int)item.PropertyInfo.MaxValue)
                                             ((int[])data)[i] = (int)item.PropertyInfo.MaxValue;
                                    }
                                 }

                                 if (item.PropertyInfo.ImposeMin)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                    {
                                       if (!use_mapping || (Mapping1D[i] == 1))
                                          if (((int[])data)[i] < (int)item.PropertyInfo.MinValue)
                                             ((int[])data)[i] = (int)item.PropertyInfo.MinValue;
                                    }
                                 }

                                 if (item.PropertyInfo.DoShiftHoriz)
                                 {
                                 }

                                 if (item.PropertyInfo.DoShiftVert)
                                 {
                                 }

                                 H5D.write(o_dsid, obj.NativeType, H5Aint);

                              }

                              for (i = 0; i < obj.Dims[0]; i++)
                              {
                                 if (!use_mapping || (Mapping1D[i] == 1))
                                 {
                                    if ((float)((int[])data)[i] <= d_min)
                                       d_min = (float)((int[])data)[i];
                                    if ((float)((int[])data)[i] >= d_max)
                                       d_max = (float)((int[])data)[i];
                                    if ((float)((int[])data)[i] <= g_min)
                                       g_min = (float)((int[])data)[i];
                                    if ((float)((int[])data)[i] >= g_max)
                                       g_max = (float)((int[])data)[i];
                                 }
                              }

                              if (Units.ContainsKey((item.Path + name).ToLower()))
                                 units = Units[(item.Path + name).ToLower()].AsString();

                              WriteAttributes(o_dsid, d_min, d_max, units);
                              break;
                        }
                        break;
                     case 2:
                        switch (obj.Class)
                        {
                           case H5T.H5TClass.FLOAT:
                              if (obj.Size == 4)
                              {
                                 //data = (float[,])new float[obj.Dims[0], obj.Dims[1]];
                                 H5D.read(i_dsid, obj.NativeType, H5Afloat);
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, H5Afloat);
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                    units = item.PropertyInfo.Units;

                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       //because the property is another, it's necessary to find the dataset name by it's index.
                                       //item.input2.Name is the property name
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       //float[,] data2 = new float[obj.Dims[0],obj.Dims[1]];
                                       H5D.read(i_dsid_2, obj.NativeType, H5Afloat2);

                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for(j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i,j] == 1))
                                                ((float[,])data)[i, j] = (float)Math.Sqrt(Math.Pow(((float[,])data)[i, j], 2) + Math.Pow(((float[,])data2)[i, j], 2));
                                          }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);

                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                             ((float[,])data)[i,j] = ((float[,])data)[i,j] * (float)4.87 / ((float)Math.Log(67.8 * 10.0 + 5.42));
                                          }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i,j] == 1))
                                                ((float[,])data)[i,j] = ((float[,])data)[i,j] * (float)item.PropertyInfo.MultFactor;
                                          }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i,j] == 1))
                                                ((float[,])data)[i,j] = ((float[,])data)[i,j] + (float)item.PropertyInfo.ValueToAdd;
                                          }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i,j] == 1))
                                                if (((float[,])data)[i,j] > (float)item.PropertyInfo.MaxValue)
                                                   ((float[,])data)[i,j] = (float)item.PropertyInfo.MaxValue;
                                          }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i,j] == 1))
                                                if (((float[,])data)[i,j] < (float)item.PropertyInfo.MinValue)
                                                   ((float[,])data)[i,j] = (float)item.PropertyInfo.MinValue;
                                          }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                       data = DoShiftHoriz((float[,])data, item.PropertyInfo.ShiftHoriz);
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    if (item.PropertyInfo.InvertHoriz)
                                    {
                                       InvertHoriz((float[,])data, (float[,])data2);
                                    }

                                    H5D.write(o_dsid, obj.NativeType, H5Afloat);
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                    for (j = 0; j < obj.Dims[1]; j++)
                                    {
                                       if (!use_mapping || (Mapping2D[i, j] == 1))
                                       {
                                          if ((float)((float[,])data)[i, j] <= d_min)
                                             d_min = (float)((float[,])data)[i, j];
                                          if ((float)((float[,])data)[i, j] >= d_max)
                                             d_max = (float)((float[,])data)[i, j];
                                          if ((float)((float[,])data)[i, j] <= g_min)
                                             g_min = (float)((float[,])data)[i, j];
                                          if ((float)((float[,])data)[i, j] >= g_max)
                                             g_max = (float)((float[,])data)[i, j];
                                       }
                                    }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              else if (obj.Size == 8)
                              {
                                 //data = (double[,])new double[obj.Dims[0], obj.Dims[1]];
                                 H5D.read(i_dsid, obj.NativeType, H5Adouble);
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, H5Adouble);
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping; 
                                    units = item.PropertyInfo.Units;
                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       //double[,] data2 = new double[obj.Dims[0], obj.Dims[1]];
                                       H5D.read(i_dsid_2, obj.NativeType, H5Adouble2);

                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                ((double[,])data)[i, j] = (double)Math.Sqrt(Math.Pow(((double[,])data)[i, j], 2) + Math.Pow(((double[,])data2)[i, j], 2));
                                          }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);

                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                ((double[,])data)[i, j] = ((double[,])data)[i, j] * (double)4.87 / ((double)Math.Log(67.8 * 10.0 + 5.42));
                                          }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                ((double[,])data)[i, j] = ((double[,])data)[i, j] * (double)item.PropertyInfo.MultFactor;
                                          }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                ((double[,])data)[i, j] = ((double[,])data)[i, j] + (double)item.PropertyInfo.ValueToAdd;
                                          }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                if (((double[,])data)[i, j] > (double)item.PropertyInfo.MaxValue)
                                                   ((double[,])data)[i, j] = (double)item.PropertyInfo.MaxValue;
                                          }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                          {
                                             if (!use_mapping || (Mapping2D[i, j] == 1))
                                                if (((double[,])data)[i, j] < (double)item.PropertyInfo.MinValue)
                                                   ((double[,])data)[i, j] = (double)item.PropertyInfo.MinValue;
                                          }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                       data = DoShiftHoriz((double[,])data, item.PropertyInfo.ShiftHoriz);
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    if (item.PropertyInfo.InvertHoriz)
                                    {
                                       InvertHoriz((double[,])data, (double[,])data2);
                                    }


                                    H5D.write(o_dsid, obj.NativeType, H5Adouble);
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                    for (j = 0; j < obj.Dims[1]; j++)
                                    {
                                       if (!use_mapping || (Mapping2D[i, j] == 1))
                                       {
                                          if ((float)((double[,])data)[i, j] <= d_min)
                                             d_min = (float)((double[,])data)[i, j];
                                          if ((float)((double[,])data)[i, j] >= d_max)
                                             d_max = (float)((double[,])data)[i, j];
                                          if ((float)((double[,])data)[i, j] <= g_min)
                                             g_min = (float)((double[,])data)[i, j];
                                          if ((float)((double[,])data)[i, j] >= g_max)
                                             g_max = (float)((double[,])data)[i, j];
                                       }
                                    }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              break;
                           case H5T.H5TClass.INTEGER:
                              //data = (int[,])new int[obj.Dims[0], obj.Dims[1]];
                              H5D.read(i_dsid, obj.NativeType, H5Aint);
                              if (item.PropertyInfo == null)
                              {
                                 H5D.write(o_dsid, obj.NativeType, H5Aint);
                                 units = "-";
                                 use_mapping = false;
                              }
                              else
                              {
                                 use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                 units = item.PropertyInfo.Units;
                                 if (item.PropertyInfo.UseComposition)
                                 {
                                    H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                    //1. Open the dataset
                                    string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                    H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                    //int[,] data2 = new int[obj.Dims[0], obj.Dims[1]];
                                    H5D.read(i_dsid_2, obj.NativeType, H5Aint2);

                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             ((int[,])data)[i, j] = (int)Math.Sqrt(Math.Pow(((int[,])data)[i, j], 2) + Math.Pow(((int[,])data2)[i, j], 2));
                                       }

                                    H5D.close(i_dsid_2);
                                    H5G.close(i_gid_2);

                                 }

                                 if (item.PropertyInfo.IsWind10To2)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             ((int[,])data)[i, j] = ((int[,])data)[i, j] * (int)4.87 / ((int)Math.Log(67.8 * 10.0 + 5.42));
                                       }
                                 }

                                 if (item.PropertyInfo.UseMultFactor)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             ((int[,])data)[i, j] = ((int[,])data)[i, j] * (int)item.PropertyInfo.MultFactor;
                                       }
                                 }

                                 if (item.PropertyInfo.UseValueToAdd)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             ((int[,])data)[i, j] = ((int[,])data)[i, j] + (int)item.PropertyInfo.ValueToAdd;
                                       }
                                 }

                                 if (item.PropertyInfo.ImposeMax)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             if (((int[,])data)[i, j] > (int)item.PropertyInfo.MaxValue)
                                                ((int[,])data)[i, j] = (int)item.PropertyInfo.MaxValue;
                                       }
                                 }

                                 if (item.PropertyInfo.ImposeMin)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                       {
                                          if (!use_mapping || (Mapping2D[i, j] == 1))
                                             if (((int[,])data)[i, j] < (int)item.PropertyInfo.MinValue)
                                                ((int[,])data)[i, j] = (int)item.PropertyInfo.MinValue;
                                       }
                                 }

                                 if (item.PropertyInfo.DoShiftHoriz)
                                 {
                                    data = DoShiftHoriz((int[,])data, item.PropertyInfo.ShiftHoriz);
                                 }

                                 if (item.PropertyInfo.DoShiftVert)
                                 {
                                 }

                                 if (item.PropertyInfo.InvertHoriz)
                                 {
                                    InvertHoriz((int[,])data, (int[,])data2);
                                 }

                                 H5D.write(o_dsid, obj.NativeType, H5Aint);

                              }

                              for (i = 0; i < obj.Dims[0]; i++)
                                 for (j = 0; j < obj.Dims[1]; j++)
                              {
                                 if (!use_mapping || (Mapping2D[i,j] == 1))
                                 {
                                    if ((float)((int[,])data)[i, j] <= d_min)
                                       d_min = (float)((int[,])data)[i, j];
                                    if ((float)((int[,])data)[i, j] >= d_max)
                                       d_max = (float)((int[,])data)[i, j];
                                    if ((float)((int[,])data)[i, j] <= g_min)
                                       g_min = (float)((int[,])data)[i, j];
                                    if ((float)((int[,])data)[i, j] >= g_max)
                                       g_max = (float)((int[,])data)[i, j];
                                 }
                              }

                              if (Units.ContainsKey((item.Path + name).ToLower()))
                                 units = Units[(item.Path + name).ToLower()].AsString();

                              WriteAttributes(o_dsid, d_min, d_max, units);
                              break;
                        }
                        break;
                     case 3:
                        switch (obj.Class)
                        {
                           case H5T.H5TClass.FLOAT:
                              if (obj.Size == 4)
                              {
                                 data = (float[,,])new float[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                 H5D.read(i_dsid, obj.NativeType, new H5Array<float>((float[,,])data));
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, new H5Array<float>((float[, ,])data));
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                    units = item.PropertyInfo.Units;
                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       float[,,] data2 = new float[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                       H5D.read(i_dsid_2, obj.NativeType, new H5Array<float>((float[,,])data2));

                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((float[, ,])data)[i, j, k] = (float)Math.Sqrt(Math.Pow(((float[,])data)[i, j], 2) + Math.Pow(data2[i, j, k], 2));
                                             }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);

                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((float[,,])data)[i, j, k] = ((float[, ,])data)[i, j, k] * (float)4.87 / ((float)Math.Log(67.8 * 10.0 + 5.42));
                                             }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((float[,,])data)[i, j, k] = ((float[, ,])data)[i, j, k] * (float)item.PropertyInfo.MultFactor;
                                             }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((float[, ,])data)[i, j, k] = ((float[, ,])data)[i, j, k] + (float)item.PropertyInfo.ValueToAdd;
                                             }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   if (((float[,,])data)[i, j, k] > (float)item.PropertyInfo.MaxValue)
                                                      ((float[,,])data)[i, j, k] = (float)item.PropertyInfo.MaxValue;
                                             }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   if (((float[,,])data)[i, j, k] < (float)item.PropertyInfo.MinValue)
                                                      ((float[,,])data)[i, j, k] = (float)item.PropertyInfo.MinValue;
                                             }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    H5D.write(o_dsid, obj.NativeType, new H5Array<float>((float[,,])data));
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                    for (j = 0; j < obj.Dims[1]; j++)
                                       for (k = 0; k < obj.Dims[1]; k++)
                                       {
                                          if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                          {
                                             if ((float)((float[, ,])data)[i, j, k] <= d_min)
                                                d_min = (float)((float[, ,])data)[i, j, k];
                                             if ((float)((float[, ,])data)[i, j, k] >= d_max)
                                                d_max = (float)((float[, ,])data)[i, j, k];
                                             if ((float)((float[, ,])data)[i, j, k] <= g_min)
                                                g_min = (float)((float[, ,])data)[i, j, k];
                                             if ((float)((float[, ,])data)[i, j, k] >= g_max)
                                                g_max = (float)((float[, ,])data)[i, j, k];
                                          }
                                       }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              else if (obj.Size == 8)
                              {
                                 data = (double[,,])new double[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                 H5D.read(i_dsid, obj.NativeType, new H5Array<double>((double[,,])data));
                                 if (item.PropertyInfo == null)
                                 {
                                    H5D.write(o_dsid, obj.NativeType, new H5Array<double>((double[, ,])data));
                                    units = "-";
                                    use_mapping = false;
                                 }
                                 else
                                 {
                                    use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                    units = item.PropertyInfo.Units;
                                    if (item.PropertyInfo.UseComposition)
                                    {
                                       H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                       //1. Open the dataset
                                       string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                       H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                       double[, ,] data2 = new double[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                       H5D.read(i_dsid_2, obj.NativeType, new H5Array<double>((double[, ,])data2));

                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((double[, ,])data)[i, j, k] = (double)Math.Sqrt(Math.Pow(((double[,])data)[i, j], 2) + Math.Pow(data2[i, j, k], 2));
                                             }

                                       H5D.close(i_dsid_2);
                                       H5G.close(i_gid_2);

                                    }

                                    if (item.PropertyInfo.IsWind10To2)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((double[, ,])data)[i, j, k] = ((double[, ,])data)[i, j, k] * (double)4.87 / ((double)Math.Log(67.8 * 10.0 + 5.42));
                                             }
                                    }

                                    if (item.PropertyInfo.UseMultFactor)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((double[, ,])data)[i, j, k] = ((double[, ,])data)[i, j, k] * (double)item.PropertyInfo.MultFactor;
                                             }
                                    }

                                    if (item.PropertyInfo.UseValueToAdd)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   ((double[, ,])data)[i, j, k] = ((double[, ,])data)[i, j, k] + (double)item.PropertyInfo.ValueToAdd;
                                             }
                                    }

                                    if (item.PropertyInfo.ImposeMax)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   if (((double[, ,])data)[i, j, k] > (double)item.PropertyInfo.MaxValue)
                                                      ((double[, ,])data)[i, j, k] = (double)item.PropertyInfo.MaxValue;
                                             }
                                    }

                                    if (item.PropertyInfo.ImposeMin)
                                    {
                                       for (i = 0; i < obj.Dims[0]; i++)
                                          for (j = 0; j < obj.Dims[1]; j++)
                                             for (k = 0; k < obj.Dims[1]; k++)
                                             {
                                                if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                   if (((double[, ,])data)[i, j, k] < (double)item.PropertyInfo.MinValue)
                                                      ((double[, ,])data)[i, j, k] = (double)item.PropertyInfo.MinValue;
                                             }
                                    }

                                    if (item.PropertyInfo.DoShiftHoriz)
                                    {
                                    }

                                    if (item.PropertyInfo.DoShiftVert)
                                    {
                                    }

                                    H5D.write(o_dsid, obj.NativeType, new H5Array<double>((double[,,])data));
                                 }

                                 for (i = 0; i < obj.Dims[0]; i++)
                                    for (j = 0; j < obj.Dims[1]; j++)
                                       for (k = 0; k < obj.Dims[1]; k++)
                                       {
                                          if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                          {
                                             if ((float)((double[, ,])data)[i, j, k] <= d_min)
                                                d_min = (float)((double[, ,])data)[i, j, k];
                                             if ((float)((double[, ,])data)[i, j, k] >= d_max)
                                                d_max = (float)((double[, ,])data)[i, j, k];
                                             if ((float)((double[, ,])data)[i, j, k] <= g_min)
                                                g_min = (float)((double[, ,])data)[i, j, k];
                                             if ((float)((double[, ,])data)[i, j, k] >= g_max)
                                                g_max = (float)((double[, ,])data)[i, j, k];
                                          }
                                       }

                                 if (Units.ContainsKey((item.Path + name).ToLower()))
                                    units = Units[(item.Path + name).ToLower()].AsString();

                                 WriteAttributes(o_dsid, d_min, d_max, units);
                              }
                              break;
                           case H5T.H5TClass.INTEGER:
                              data = (int[, ,])new int[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                              H5D.read(i_dsid, obj.NativeType, new H5Array<int>((int[,,])data));
                              if (item.PropertyInfo == null)
                              {
                                 H5D.write(o_dsid, obj.NativeType, new H5Array<int>((int[, ,])data));
                                 units = "-";
                                 use_mapping = false;
                              }
                              else
                              {
                                 use_mapping = Options.UseMapping && item.PropertyInfo.IgnoreMapping;
                                 units = item.PropertyInfo.Units;
                                 if (item.PropertyInfo.UseComposition)
                                 {
                                    H5GroupId i_gid_2 = H5G.open(input.FileID, item.Input2.Path);
                                    //1. Open the dataset
                                    string dataset_name = H5G.getObjectNameByIndex(i_gid_2, obj.Index);
                                    H5DataSetId i_dsid_2 = H5D.open(i_gid_2, dataset_name);
                                    int[, ,] data2 = new int[obj.Dims[0], obj.Dims[1], obj.Dims[2]];
                                    H5D.read(i_dsid_2, obj.NativeType, new H5Array<int>((int[, ,])data2));

                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                ((int[, ,])data)[i, j, k] = (int)Math.Sqrt(Math.Pow(((int[,])data)[i, j], 2) + Math.Pow(data2[i, j, k], 2));
                                          }

                                    H5D.close(i_dsid_2);
                                    H5G.close(i_gid_2);
                                 }

                                 if (item.PropertyInfo.IsWind10To2)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                ((int[, ,])data)[i, j, k] = ((int[, ,])data)[i, j, k] * (int)4.87 / ((int)Math.Log(67.8 * 10.0 + 5.42));
                                          }
                                 }

                                 if (item.PropertyInfo.UseMultFactor)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                ((int[, ,])data)[i, j, k] = ((int[, ,])data)[i, j, k] * (int)item.PropertyInfo.MultFactor;
                                          }
                                 }

                                 if (item.PropertyInfo.UseValueToAdd)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                ((int[, ,])data)[i, j, k] = ((int[, ,])data)[i, j, k] + (int)item.PropertyInfo.ValueToAdd;
                                          }
                                 }

                                 if (item.PropertyInfo.ImposeMax)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                if (((int[, ,])data)[i, j, k] > (int)item.PropertyInfo.MaxValue)
                                                   ((int[, ,])data)[i, j, k] = (int)item.PropertyInfo.MaxValue;
                                          }
                                 }

                                 if (item.PropertyInfo.ImposeMin)
                                 {
                                    for (i = 0; i < obj.Dims[0]; i++)
                                       for (j = 0; j < obj.Dims[1]; j++)
                                          for (k = 0; k < obj.Dims[1]; k++)
                                          {
                                             if (!use_mapping || (Mapping3D[i, j, k] == 1))
                                                if (((int[, ,])data)[i, j, k] < (int)item.PropertyInfo.MinValue)
                                                   ((int[, ,])data)[i, j, k] = (int)item.PropertyInfo.MinValue;
                                          }
                                 }

                                 if (item.PropertyInfo.DoShiftHoriz)
                                 {
                                 }

                                 if (item.PropertyInfo.DoShiftVert)
                                 {
                                 }

                                 H5D.write(o_dsid, obj.NativeType, new H5Array<int>((int[,,])data));
                              }

                              for (i = 0; i < obj.Dims[0]; i++)
                                 for (j = 0; j < obj.Dims[1]; j++)
                                    for (k = 0; k < obj.Dims[1]; k++)
                                    {
                                       if (!use_mapping || (Mapping3D[i,j,k] == 1))
                                       {
                                          if ((float)((int[,,])data)[i,j,k] <= d_min)
                                             d_min = (float)((int[,,])data)[i,j,k];
                                          if ((float)((int[,,])data)[i,j,k] >= d_max)
                                             d_max = (float)((int[,,])data)[i,j,k];
                                          if ((float)((int[,,])data)[i,j,k] <= g_min)
                                             g_min = (float)((int[,,])data)[i,j,k];
                                          if ((float)((int[,,])data)[i,j,k] >= g_max)
                                             g_max = (float)((int[,,])data)[i,j,k];
                                       }
                                    }

                              if (Units.ContainsKey((item.Path + name).ToLower()))
                                 units = Units[(item.Path + name).ToLower()].AsString();

                              WriteAttributes(o_dsid, d_min, d_max, units);
                              break;
                        }
                        break;
                  }                  

                  H5D.close(i_dsid);
                  H5G.close(i_gid);


                  H5P.close(o_pid);
                  H5P.close(o_pid_acc);
                  H5P.close(o_pid_l); 
                  H5D.close(o_dsid);
                  H5S.close(o_sid);
                  H5G.close(o_gid);                                          
               }
            }

            WriteAttributes((H5GroupId)id, g_min, g_max);
         }
         else
         {
            WriteAttributes((H5GroupId)id, g_min, g_max);

            foreach (HDFOutputObjectInfo oi in item.Children)
            {
               bool exist = false;
               H5GroupId new_id = null;

               try
               {
                  new_id = H5G.open(id, oi.Name);
                  if (new_id.Id >= 0)
                     exist = true;
                  else
                     exist = false;
               }
               catch
               {
                  exist = false;
               }

               if (!exist)
                  new_id = H5G.create(id, oi.Name);

               if (new_id == null || new_id.Id < 0)
                  throw new Exception("Was not possible to create the group '" + oi.Name + "'");

               WriteStructure(oi, new_id);

               H5G.close(new_id);
            }
         }
      }

      protected T[,] DoShiftHoriz<T>(T[,] data, int steps)
      {
         try
         {
            int i_max = data.GetLength(0);
            int j_max = data.GetLength(1);
            int i, j, j_a;

            T[,] shift_data = new T[i_max, j_max];

            if (steps >= 0) //shift right
            {
               for (i = 0; i < i_max; i++)
               {
                  for (j = 0; j < j_max - steps; j++)
                  {
                     shift_data[i, j + steps] = data[i, j];
                  }

                  for (j_a = 0; j_a < steps; j_a++, j++)
                  {
                     shift_data[i, j_a] = data[i, j];
                  }
               }
            }
            else if (steps < 0) //shift left
            {
               for (i = 0; i < i_max; i++)
               {
                  for (j = j_max - 1; j >= steps; j--)
                  {
                     shift_data[i, j - steps] = data[i, j];
                  }

                  for (j_a = steps - 1; j_a >= 0; j_a--, j--)
                  {
                     shift_data[i, j_a] = data[i, j];
                  }
               }
            }

            return shift_data;
         }
         catch (Exception ex)
         {
            last_exception = ex;
            return null;
         }
      }

      protected void InvertHoriz<T>(T[,] data, T[,] data2)
      {
         try
         {
            int i_max = data.GetLength(0);
            int j_max = data.GetLength(1);
            int i, j, j_a;

            //T[,] inv_data = new T[i_max, j_max];

            for (i = 0; i < i_max; i++)
            {
               for (j = 0, j_a = j_max - 1; j < j_max; j++, j_a--)
               {
                  data2[i, j_a] = data[i, j];
               }

               for (j = 0; j < j_max; j++)
               {
                  data[i, j] = data2[i, j];
               }

            }
         }
         catch (Exception ex)
         {
            last_exception = ex;
         }
      }

      protected void WriteData(HDFOutputObjectInfo item, H5LocId id)
      {
         //H5GroupId oid;

         //foreach (HDFOutputObjectInfo oi in item.Children)
         //{
            

         //   if (oi.IsProperty)
         //   {
         //      oid = H5G.open(id, item.Name);
         //      H5G.close(oid);
         //   }
         //   else if (oi.Type == H5GType.DATASET)
         //   {
         //      H5D.open(id, item.Name);
         //      H5D.close(
         //   }
         //   else
         //   {
         //      oid = H5G.open(id, item.Name);
         //      WriteData(oi, oid);
         //      H5G.close(oid);
         //   }
         //}
      }

      protected void CreateHDF()
      {
         if (!output.CreateHDF(Options.Output))
            throw new Exception("Failed to create the HDF");
      }

      //protected void WriteGroup(string[] tokens, int index, H5GroupId id)
      //{
      //   if (index < tokens.Length)
      //   {
      //      bool exist = false;
      //      H5GroupId new_id = null;
      //      string group = tokens[index];

      //      try
      //      {
      //         new_id = H5G.open(id, group);
      //         if (new_id.Id >= 0)
      //            exist = true;
      //         else
      //            exist = false;
      //      }
      //      catch
      //      {
      //         exist = false;
      //      }

      //      if (!exist)
      //         new_id = H5G.create(id, group);

      //      if (new_id == null || new_id.Id < 0)
      //         throw new Exception("Was not possible to create the group '" + group + "'");

      //      index++;
      //      WriteGroup(tokens, index, new_id);

      //      H5G.close(new_id);
      //   }
      //}

      //protected void WriteGroups()
      //{         
      //   int index;
      //   H5GroupId id = H5G.open(output.FileID, "/");

      //   foreach (TaskGroupInfo gr in group_list)
      //   {
      //      string[] tokens = gr.OutputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);                       
      //      index = 0;
      //      WriteGroup(tokens, index, id);
      //   }

      //   H5G.close(id);
      //}

      //protected void WriteProperties()
      //{
      //   foreach (TaskGroupInfo gr in group_list)
      //   {
      //      if (gr.SaveAll)
      //      {
      //         string[] tokens = gr.InputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);


      //      }
      //   }
      //}

      protected void CheckGroupsInfo()
      {
         StringBuilder errors = new StringBuilder();
         bool there_are_errors = false;

         errors.AppendLine("One or more Group Info blocks failed");

         foreach (TaskGroupInfo gr in group_list)
         {            
            string[] tokens = gr.InputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            List<HDFObjectInfo> list = input_root.Children;
            foreach (string group in tokens)
            {
               bool found = false;
               foreach (HDFObjectInfo obj in list)
               {
                  if (obj.Type == H5GType.GROUP && obj.Name == group.Trim())
                  {
                     found = true;
                     list = obj.Children;
                     break;
                  }
               }
               if (!found)
               {
                  errors.AppendLine("Input group path '" + gr.InputPath + "' doesn't exist.");
                  there_are_errors = true;
               }
            }
         }

         if (there_are_errors)
            throw new Exception(errors.ToString());
      }

      protected void LoadGlobalConfig(ConfigNode cfg)
      {
         Options.DateTimeFormat = cfg["date.time.format", "dd/MM/yyyy"].AsString();
         //Options.DefineGridGroup = cfg["grid.group", true].AsBool();
         //Options.DefineResultsGroup = cfg["results.group", true].AsBool();
         //Options.DefineTimeGroup = cfg["time.group", true].AsBool();
         Options.Input = cfg["input.hdf"].AsFileName();
         Options.Output = cfg["output.hdf"].AsFileName();

         Options.UseMapping = cfg.NodeData.ContainsKey("mapping.points");
         if (Options.UseMapping)
            Options.Mapping = cfg["mapping.points"].AsString();
         
         //Options.UseStart = cfg.NodeData.ContainsKey("start");
         //Options.UseEnd = cfg.NodeData.ContainsKey("end");
         //if (Options.UseStart)
         //   Options.Start = cfg["start"].AsDateTime(Options.DateTimeFormat);
         //if (Options.UseEnd)
         //   Options.Start = cfg["end"].AsDateTime(Options.DateTimeFormat);
      }

      public void LoadGroupsToSave(ConfigNode cfg)
      {
         //bool grid_group_exists = false,
         //     time_group_exists = false,
         //     results_group_exists = false;
         string[] separator = { "," };

         ConfigNode groups_to_save = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "groups.to.save"; });
         foreach (KeywordData group in groups_to_save.NodeData.Values)
         {            
            TaskGroupInfo tgi = new TaskGroupInfo();

            string[] tokens = group.AsString().Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1)
               throw new Exception("Invlaid entry in 'groups.to.save' block");
            
            tgi.InputPath = tokens[0].Trim();
            
            if (tokens.Length > 1)
               tgi.OutputPath = tokens[1].Trim();
            else
               tgi.OutputPath = tokens[0].Trim();
            
            group_list.Add(tgi);

            //if (tgi.InputPath == "/Results/") results_group_exists = true;
            //if (tgi.InputPath == "/Grid/") grid_group_exists = true;
            //if (tgi.InputPath == "/Time/") time_group_exists = true;
         }

         //if (!grid_group_exists && Options.DefineGridGroup)
         //{
         //   TaskGroupInfo tgi = new TaskGroupInfo();

         //   tgi.InputPath = "/Grid/";
         //   tgi.OutputPath = "/Grid/";
         //   tgi.SaveAll = true;

         //   group_list.Add(tgi);
         //}

         //if (!time_group_exists && Options.DefineTimeGroup)
         //{
         //   TaskGroupInfo tgi = new TaskGroupInfo();

         //   tgi.InputPath = "/Time/";
         //   tgi.OutputPath = "/Time/";
         //   tgi.SaveAll = true;

         //   group_list.Add(tgi);
         //}

         //if (!results_group_exists && Options.DefineResultsGroup)
         //{
         //   TaskGroupInfo tgi = new TaskGroupInfo();

         //   tgi.InputPath = "/Results/";
         //   tgi.OutputPath = "/Results/";
         //   tgi.SaveAll = true;

         //   group_list.Add(tgi);
         //}
      }

      protected void LoadProcessingInfo(ConfigNode cfg)
      {
         List<ConfigNode> groups = cfg.ChildNodes.FindAll(delegate(ConfigNode node) { return node.Name == "process.info"; });
         foreach (ConfigNode item in groups)
         {
            TaskPropertyInfo tpi = new TaskPropertyInfo();

            tpi.OutputName = item["output.name"].AsString();
            tpi.OutputPath = item["output.path", "/Results/" + tpi.OutputName].AsString();

            if (item.NodeData.ContainsKey("input.name"))
               tpi.InputName = item["input.name"].AsString();
            else
               tpi.InputName = tpi.OutputName;
            if (item.NodeData.ContainsKey("input.path"))
               tpi.InputPath = item["input.path"].AsString();
            else
               tpi.InputPath = tpi.OutputPath;

            if (item.NodeData.ContainsKey("input.name.2"))
            {
               tpi.UseComposition = true;
               
               tpi.InputName2 = item["input.name.2"].AsString();
               tpi.InputPath2 = item["input.path.2"].AsString();
            }
            else            
               tpi.UseComposition = false;            

            tpi.UseMultFactor = item.NodeData.ContainsKey("mult.factor");
            if (tpi.UseMultFactor)
               tpi.MultFactor = item["mult.factor"].AsDouble();

            tpi.UseValueToAdd = item.NodeData.ContainsKey("add.value");
            if (tpi.UseValueToAdd)
               tpi.ValueToAdd = item["add.value"].AsDouble();

            tpi.IsWind10To2 = item["wind.10.to.2", false].AsBool();

            tpi.ImposeMin = item.NodeData.ContainsKey("min");
            if (tpi.ImposeMin)
               tpi.MinValue = item["min"].AsDouble();

            tpi.ImposeMax = item.NodeData.ContainsKey("max");
            if (tpi.ImposeMax)
               tpi.MaxValue = item["max"].AsDouble();

            tpi.DoShiftHoriz = item.NodeData.ContainsKey("shift.horiz");
            if (tpi.DoShiftHoriz)
               tpi.ShiftHoriz = item["shift.horiz"].AsInt();

            tpi.DoShiftVert = item.NodeData.ContainsKey("shift.vert");
            if (tpi.DoShiftVert)
               tpi.ShiftVert = item["shift.vert"].AsInt();

            tpi.InvertHoriz = item["invert.horiz", false].AsBool();
            tpi.InvertVert = item["invert.vert", false].AsBool();
            tpi.IgnoreMapping = item["ignore.mapping", false].AsBool();
            tpi.Units = item["units", "-"].AsString();

            property_list.Add(tpi);
         }
      }

      protected void LoadExceptions(ConfigNode cfg)
      {
         ConfigNode group = cfg.ChildNodes.Find(delegate(ConfigNode node) { return node.Name == "not.to.save"; });
         if (group != null)
            foreach (KeywordData item in group.NodeData.Values)
            {
               not_to_save.Add(item.AsString());
            }
      }

      protected void LinkProperties()
      {
         //int index = 1;
         //foreach (TaskPropertyInfo pi in property_list)
         //{
         //   string[] i_tokens = pi.InputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);
         //   string[] o_tokens = pi.OutputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries);

         //   if (i_tokens.Length != o_tokens.Length)
         //      throw new Exception("The paths to the property '" + index + "' are not the same.");
            
         //   for(int i = 0; i < (i_tokens.Length - 1); i++)
         //      if (i_tokens[i] != o_tokens[i])
         //         throw new Exception("The paths to the property '" + index + "' are not the same.");

         //   HDFObjectInfo item = FindItem(i_tokens, hdf_input_structure, 0);
         //   item.Info = pi;
         //}
      }

      protected HDFObjectInfo FindItem(string[] tokens, List<HDFObjectInfo> list, int index)
      {
         if (index < tokens.Length)
         {
            HDFObjectInfo item = list.Find(delegate(HDFObjectInfo oi) { return oi.Name == tokens[index]; });
            if (item != null)
            {
               if (item.IsProperty)
                  return item;
               else
               {                  
                  index++;
                  return FindItem(tokens, item.Children, index);
               }
            }
            else
               throw new Exception("Property path not found.");
         }

         return null;
      }

      protected HDFObjectInfo FindItem(string path, List<HDFObjectInfo> list)
      {
         foreach (HDFObjectInfo obj in list)
         {
            if (obj.Path == path)
               return obj;
            else
            {
               HDFObjectInfo obj2 = FindItem(path, obj.Children);
               if (obj2 != null)
                  return obj2;
            }
         }

         return null;
      }

      protected void ReadInputHDFStructure()
      {
         input.InitializeLibrary();

         input.OpenHDF(Options.Input, H5F.OpenMode.ACC_RDONLY);
         input.GetTree(input_root, input.FileID, "/", "/");
      }

      protected void CreateOutputStructure()
      {
         foreach (TaskGroupInfo gi in group_list)
         {
            HDFObjectInfo input = FindItem(gi.InputPath, input_root.Children);
            HDFOutputObjectInfo output = CreateOutputItem(gi.OutputPath.Split(separator, StringSplitOptions.RemoveEmptyEntries), 0, output_root);
            ConstructTreeForOutputItem(input, output);
            SetupPropertiesToProcess(output_root);
         }
      }

      protected void SetupPropertiesToProcess(HDFOutputObjectInfo oi)
      {
         foreach (HDFOutputObjectInfo item in oi.Children)
         {
            foreach (TaskPropertyInfo pi in property_list)
            {
               if (item.Path == pi.OutputPath)
               {
                  item.PropertyInfo = pi;
                  if (pi.UseComposition)
                  {
                     HDFObjectInfo input2 = FindItem(pi.InputPath2, input_root.Children);
                     item.Input2 = input2;
                  }

                  break;
               }
            }

            SetupPropertiesToProcess(item);
         }
      }

      protected void ConstructTreeForOutputItem(HDFObjectInfo input, HDFOutputObjectInfo output)
      {
         bool ignore, to_process;
         TaskPropertyInfo process_item = null;
         HDFOutputObjectInfo new_o = null;

         output.Input = input;

         foreach (HDFObjectInfo i in input.Children)
         {
            if (i.Type != H5GType.GROUP)
               continue;

            ignore = false;

            foreach (string path in not_to_save)
            {
               if (path == i.Path)
               {
                  ignore = true;
                  break;
               }
            }

            to_process = false;

            foreach (TaskPropertyInfo pi in property_list)
            {
               if (pi.InputPath == i.Path)
               {
                  process_item = pi;
                  to_process = true;
               }
            }

            if (to_process)
            {
               new_o = new HDFOutputObjectInfo();
               new_o.Name = process_item.OutputName;
               new_o.Parent = output;
               new_o.IsProperty = i.IsProperty;
               new_o.Type = i.Type;
               new_o.Path = new_o.Parent.Path + new_o.Name + "/";
               output.Children.Add(new_o);

               ConstructTreeForOutputItem(i, new_o);
            }
            else if (!ignore)
            {
               new_o = new HDFOutputObjectInfo();
               new_o.Name = i.Name;
               new_o.Parent = output;
               new_o.IsProperty = i.IsProperty;
               new_o.Type = i.Type;
               new_o.Path = new_o.Parent.Path + new_o.Name + "/";
               output.Children.Add(new_o);

               ConstructTreeForOutputItem(i, new_o);
            }
         }
      }

      protected HDFOutputObjectInfo CreateOutputItem(string[] path, int index, HDFOutputObjectInfo parent)
      {
         if (index < path.Length)
         {
            HDFOutputObjectInfo new_oi = null;

            foreach (HDFOutputObjectInfo oi in parent.Children)
            {
               if (oi.Name == path[index])
               {
                  new_oi = oi;
                  break;
               }
            }

            if (new_oi == null)
            {
               new_oi = new HDFOutputObjectInfo();
               new_oi.Name = path[index];
               new_oi.Parent = parent;                
               new_oi.Path = parent.Path + new_oi.Name + "/";
               parent.Children.Add(new_oi);
            }

            index++;
            if (index == path.Length)            
               return new_oi;

            return CreateOutputItem(path, index, new_oi);
         }

         return null;
      }
   }
}
