using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;

using LearnGamify.Models;

namespace LearnGamify.Services.Base
{
    /// <summary>
    /// 父類別
    /// </summary>
    public class ServiceBase_Database
    {
        public LearnGamifyContext _context { get; set; } 

        public ServiceBase_Database(LearnGamifyContext objContext)
        {
            _context = objContext;
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="entity">類別</param>
        /// <returns></returns>
        public async Task CreateBase<T>(T entity) where T : class
        {
            try
            {
                // 處理 MySQL 特殊字元
                SanitizeEntityProperties(entity);

                // 交易 
                _context.Set<T>().Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string sErrorMsg = ex.Message;

                _context.Entry<T>(entity).State = EntityState.Detached;
                throw;
            }
            finally
            {
                // 釋放 Table
                _context.Entry<T>(entity).State = EntityState.Detached;
            }
        }


        /// <summary>
        /// 修改資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entity">類別</param>
        /// <returns></returns>
        public async Task UpdateBase<T>(T entity) where T : class
        {
            // 處理 MySQL 特殊字元
            SanitizeEntityProperties(entity);

            try
            {
                _context.Update(entity);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _context.Entry<T>(entity).State = EntityState.Detached;
                throw;
            }
            finally
            {
                // 釋放 Table
                _context.Entry<T>(entity).State = EntityState.Detached;
            }
        }


        /// <summary>
        /// 刪除資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entity">類別</param>
        /// <returns></returns>
        public async Task DeleteBase<T>(T entity) where T : class
        {
            try
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch
            {
                _context.Entry<T>(entity).State = EntityState.Detached;
                throw;
            }
            finally
            {
                // 釋放 Table
                _context.Entry<T>(entity).State = EntityState.Detached;
            }
        }

        /// <summary>
        /// 處理實體屬性中的特殊字元
        /// </summary>
        /// <typeparam name="T">實體類型</typeparam>
        /// <param name="entity">實體資料</param>
        private void SanitizeEntityProperties<T>(T entity) where T : class
        {
            if (entity == null)
                return;

            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                // 檢查屬性是否為字串類型，且可以讀寫
                if (property.PropertyType == typeof(string) && property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(entity) as string;
                    if (value != null)
                    {
                        // 替換單引號為雙單引號
                        property.SetValue(entity, value.Replace("'", "\'"));
                    }
                }
            }
        }
        /// <summary> 
        /// 取得資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="ErrMsg">錯誤訊息</param>
        /// <param name="filter">LINQ 表達式</param>
        /// <returns></returns>
        public IQueryable<T> LookupBase<T>(Expression<Func<T, bool>>? filter = null) where T : class
        {
            /* 標記為 NoTracking，防止對取出 Items 做變更後接續 SaveChanges() 動作一併儲存 */
            var dataList = _context.Set<T>().AsNoTracking();

            if (filter != null)
            {
                dataList = dataList.Where(filter);
            }

            return dataList;

        }
    }
}
