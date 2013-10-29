using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mohid;
using Mohid.Files;
using HDF5DotNet;

namespace Mohid
{
   namespace HDF
   {
      public class HDFObjectInfo
      {
         public string Name;
         public string Path;
         public ulong Index;
         public H5GType Type;
         public bool IsProperty;
         public bool HasDatasets;

         public HDFObjectInfo Parent;
         public List<HDFObjectInfo> Children;

         public int NDims;
         public long[] Dims, MaxDims;
         public long NItems;
         public H5DataTypeId DataTypeId, NativeType;
         public int Size;
         public H5T.H5TClass Class;


         public HDFObjectInfo()
         {
            Children = new List<HDFObjectInfo>();
            Name = "";
            IsProperty = false;
            HasDatasets = false;
         }
      }

      public class HDF
      {
         public bool UseMohidAttributes;
         static protected bool library_initialized = false;
         protected H5FileId file_id;
         protected Exception last_exception;

         public HDF()
         {
            file_id = null;
            last_exception = null;
            UseMohidAttributes = true;
         }

         public bool IsLibraryInitialized
         {
            get
            {
               return library_initialized;
            }
         }

         public Exception LastException
         {
            get
            {
               Exception to_return = last_exception;
               last_exception = null;
               return to_return;
            }
         }

         public H5FileId FileID
         {
            get
            {
               return file_id;
            }
         }

         public bool InitializeLibrary()
         {
            try
            {
               if (!library_initialized)
                  if (H5.Open() >= 0)
                     library_initialized = true;
               last_exception = null;
               H5E.suppressPrinting();
               return library_initialized;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return false;
            }
         }

         public bool CloseLibrary()
         {
            try
            {
               if (library_initialized)
                  if (H5.Close() >= 0)
                     library_initialized = false;
               last_exception = null;
               return !library_initialized;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return false;
            }
         }

         public bool CreateHDF(FileName file, H5F.CreateMode mode = H5F.CreateMode.ACC_TRUNC)
         {
            try
            {
               file_id = H5F.create(file.FullPath, mode);
               last_exception = null;
               if (file_id.Id >= 0)
                  return true;
               else
                  return false;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return false;
            }
         }

         public bool OpenHDF(FileName file, H5F.OpenMode mode = H5F.OpenMode.ACC_RDONLY)
         {
            try
            {
               file_id = H5F.open(file.FullPath, mode);
               last_exception = null;
               if (file_id.Id >= 0)
                  return true;
               else
                  return false;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return false;
            }
         }

         public bool CloseHDF()
         {
            try
            {
               H5F.close(file_id);
               last_exception = null;
               return true;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return false;
            }
         }

         public List<HDFObjectInfo> GetRootObjects()
         {
            try
            {
               List<HDFObjectInfo> list = new List<HDFObjectInfo>();

               H5GroupId root_id = H5G.open(file_id, "/");
               long n_objs = H5G.getNumObjects(root_id);

               for (ulong i = 0; i < (ulong)n_objs; i++)
               {
                  HDFObjectInfo info = new HDFObjectInfo();

                  info.Index = i;
                  info.Name = H5G.getObjectNameByIndex(root_id, i);
                  ObjectInfo h5oi = H5G.getObjectInfo(root_id, info.Name, false);
                  info.Type = h5oi.objectType;

                  list.Add(info);
               }

               H5G.close(root_id);

               return list;
            }
            catch (Exception ex)
            {
               last_exception = ex;
               return null;
            }
         }

         public void GetTree(HDFObjectInfo parent, H5LocId id, string name, string path)
         {
            try
            {
               H5GroupId root_id = H5G.open(id, name);
               parent.NItems = H5G.getNumObjects(root_id);

               for (ulong i = 0; i < (ulong)parent.NItems; i++)
               {
                  HDFObjectInfo info = new HDFObjectInfo();

                  info.Index = i;
                  info.Name = H5G.getObjectNameByIndex(root_id, (ulong)i);
                  ObjectInfo h5oi = H5G.getObjectInfo(root_id, info.Name, false);
                  info.Type = h5oi.objectType;
                  info.Path = path;

                  if (info.Type == H5GType.GROUP)
                  {
                     info.Path += info.Name + "/";
                     info.Parent = parent;
                     GetTree(info, root_id, info.Name, info.Path);
                     info.IsProperty = true;

                     foreach (HDFObjectInfo obj in info.Children)
                     {
                        obj.Parent = info;

                        if (obj.Type != H5GType.DATASET)
                        {
                           info.IsProperty = false;
                           break;
                        }
                     }
                  }
                  else
                  {
                     info.Path += info.Name;
                     info.Parent = parent;

                     if (info.Type == H5GType.DATASET)
                     {
                        H5DataSetId dsid = H5D.open(root_id, info.Name);
                        H5DataSpaceId dspid = H5D.getSpace(dsid);

                        info.DataTypeId = H5D.getType(dsid);
                        info.Size = H5T.getSize(info.DataTypeId);
                        info.Class = H5T.getClass(info.DataTypeId);
                        info.NativeType = H5T.getNativeType(info.DataTypeId, H5T.Direction.DESCEND);
                        info.Dims = H5S.getSimpleExtentDims(dspid);
                        info.NDims = H5S.getSimpleExtentNDims(dspid);
                        info.MaxDims = H5S.getSimpleExtentMaxDims(dspid);
                        info.Parent.HasDatasets = true;

                        H5S.close(dspid);
                        H5D.close(dsid);
                     }
                  }

                  parent.Children.Add(info);
               }

               H5G.close(root_id);
            }
            catch (Exception ex)
            {
               last_exception = ex;
            }
         }
      }
   }
}