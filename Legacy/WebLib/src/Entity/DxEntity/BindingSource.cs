using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ai.Entity.DxEntity
{
    // DevExpress.Data.EntityBindingSource : Component, IListSource
    public class DxBindingSource : System.ComponentModel.Component, IListSource
    {
        System.Collections.IList list = null;
        object dataSource;
        public virtual object DataSource
        {
            get { return dataSource; }
            set
            {
                if (dataSource != value)
                {
                    dataSource = value;
                    list = value as IList;
                }
            }
        }
        protected virtual bool CacheList { get { return true; } }
        #region IListSource Members
        bool IListSource.ContainsListCollection
        {
            get { return false; }
        }
        IList IListSource.GetList()
        {
            if (list == null || !CacheList)
                list = Site != null ? GetDesigntimeList() : GetRuntimeList();
            return list;
        }
        protected virtual IList GetDesigntimeList()
        {
            if (dataSource is Type)
            {
                Type listType = typeof(List<>).MakeGenericType((Type)dataSource);
                return (IList)Activator.CreateInstance(listType);
            }
            return null;
        }
        protected virtual IList GetRuntimeList()
        {
            return null;
        }
        #endregion
    }
    

    public interface IListAdapter
    {
        void FillList(IServiceProvider servProvider);
    }


}
