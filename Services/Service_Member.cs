using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using LearnGamify.MemberDto;
using LearnGamify.Models;
using LearnGamify.ResponseBaseDto;
using LearnGamify.Services.Base;

namespace LearnGamify.Services
{
    public partial class Service_Member : ServiceBase_Database
    {

        /// <summary>
        /// 建構
        /// </summary>
        /// <param name="objContext"></param>
        public Service_Member(
            LearnGamifyContext context
            ) : base(context)
        {
        }

        public IDbContextTransaction GetTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        /// <summary>
        /// 會員新增
        /// </summary>
        /// <param name="_request"></param>
        /// <returns></returns>
        internal async Task<Responser> Create(RequestDto.Create? _request)
        {
            Responser _responser = new Responser();
            try
            {
                using (var transaction = this.GetTransaction())
                {
                    Member _db = new Member()
                    {
                        Name = _request.name,
                    };
                    await base.CreateBase(_db);
                    await transaction.CommitAsync();
                    _responser.msg = "成功";
                }
            }
            catch (Exception ex)
            {
                _responser.result = false;
                _responser.statusCode = 500;
                _responser.msg = ex.Message;
                return _responser;
            }

            return _responser;
        }

        internal async Task<ResponserT<List<ResponseDto.ReadList>>> ReadList(RequestDto.ReadList _request)
        {
            ResponserT<List<ResponseDto.ReadList>> _responser = new ResponserT<List<ResponseDto.ReadList>>();
            try
            {
                using (var transaction = this.GetTransaction())
                {
                    var _db = await _context.Member
                        .AsNoTracking()
                        .Select(x => new ResponseDto.ReadList
                        {
                            id = x.Id,
                            name = x.Name,
                        })
                        .ToListAsync();

                    // 篩選
                    if (_request != null)
                        if (_request.id >= 1)
                            _db = _db.Where(x => x.id == _request.id).ToList();


                    if (!_db.Any())
                    {
                        _responser.msg = "找不到資料";
                        return _responser;
                    }

                    _responser.data = _db;
                }
            }
            catch (Exception ex)
            {
                _responser.result = false;
                _responser.statusCode = 500;
                _responser.msg = ex.Message;
                return _responser;
            }

            return _responser;
        }

        internal async Task<Responser> Update(RequestDto.Update? _request)
        {
            Responser _responser = new Responser();
            try
            {
                using (var transaction = this.GetTransaction())
                {
                    var _db = await _context.Member.AsNoTracking().Where(x => x.Id == _request.id).FirstOrDefaultAsync();
                    if (_db == null)
                    {
                        _responser.result = false;
                        _responser.statusCode = 404;
                        _responser.msg = "找不到資料";
                        return _responser;
                    }

                    _db.Name = _request.name;

                    await UpdateBase(_db);
                    await transaction.CommitAsync();
                    _responser.msg = "成功";
                }
            }
            catch (Exception ex)
            {
                _responser.result = false;
                _responser.statusCode = 500;
                _responser.msg = ex.Message;
                return _responser;
            }

            return _responser;
        }

        internal async Task<Responser> Delete(RequestDto.Delete? _request)
        {
            Responser _responser = new Responser();
            try
            {
                using (var transaction = this.GetTransaction())
                {
                    var _db = await _context.Member.AsNoTracking().Where(x => x.Id == _request.id).FirstOrDefaultAsync();
                    if (_db == null)
                    {
                        _responser.result = false;
                        _responser.statusCode = 404;
                        _responser.msg = "找不到資料";
                        return _responser;
                    }

                    await DeleteBase(_db);
                    await transaction.CommitAsync();
                    _responser.msg = "成功";
                }
            }
            catch (Exception ex)
            {
                _responser.result = false;
                _responser.statusCode = 500;
                _responser.msg = ex.Message;
                return _responser;
            }

            return _responser;
        }
    }
}