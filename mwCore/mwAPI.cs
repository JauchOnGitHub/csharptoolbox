using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Mohid
{
   namespace Workspace
   {
      public interface mwAPI
      {
         public bool AddNewMainMenu(ToolStripMenuItem newItem);
         public bool AddNewMenuToToolsMenu(ToolStripMenuItem newItem);

         public int AddNewToolStrip();
         public bool AddNewToolStripItem(int id, ToolStripButton newItem);
         public bool AddNewToolStripItem(int id, ToolStripDropDownButton newItem);
         public bool AddNewToolStripItem(int id, ToolStripLabel newItem);
         public bool AddNewToolStripItem(int id, ToolStripSplitButton newItem);
         public bool AddNewToolStripItem(int id, ToolStripSeparator newItem);
         public bool AddNewToolStripItem(int id, ToolStripComboBox newItem);
         public bool AddNewToolStripItem(int id, ToolStripTextBox newItem);
         public bool AddNewToolStripItem(int id, ToolStripProgressBar newItem);

         public mwTreeNode GetSelectedNode();
      }
   }
}
