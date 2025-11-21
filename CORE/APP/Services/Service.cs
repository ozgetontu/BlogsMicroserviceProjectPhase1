using CORE.APP.Domain;
using CORE.APP.Models;
using Microsoft.EntityFrameworkCore;

namespace CORE.APP.Services
{
    // ServiceBase kaldırıldı (çünkü o dosya bizde yok), IDisposable tutuldu.
    public abstract class Service<TEntity> : IDisposable where TEntity : Entity, new()
    {
        private readonly DbContext _db;

        protected Service(DbContext db)
        {
            _db = db;
        }

        // *** Synchronous Repository Operations ***

        protected virtual IQueryable<TEntity> Query(bool isNoTracking = true)
        {
            return isNoTracking ? _db.Set<TEntity>().AsNoTracking() : _db.Set<TEntity>();
        }

        protected virtual int Save() => _db.SaveChanges();

        protected void Create(TEntity entity, bool save = true)
        {
            entity.Guid = Guid.NewGuid().ToString();
            _db.Set<TEntity>().Add(entity);
            if (save)
                Save();
        }

        protected void Update(TEntity entity, bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                Save();
        }

        protected void Delete(TEntity entity, bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                Save();
        }

        // *** Asynchronous Repository Operations ***

        protected virtual async Task<int> Save(CancellationToken cancellationToken) => await _db.SaveChangesAsync(cancellationToken);

        protected async Task Create(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            entity.Guid = Guid.NewGuid().ToString();
            _db.Set<TEntity>().Add(entity);
            if (save)
                await Save(cancellationToken);
        }

        protected async Task Update(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Update(entity);
            if (save)
                await Save(cancellationToken);
        }

        protected async Task Delete(TEntity entity, CancellationToken cancellationToken, bool save = true)
        {
            _db.Set<TEntity>().Remove(entity);
            if (save)
                await Save(cancellationToken);
        }

        // *** Relational Data Operations (HATA BURADAYDI) ***
        // Hoca burada 'where TRelationalEntity : Entity' demişti.
        // Biz bunu 'where TRelationalEntity : class' yaptık.
        // Böylece BlogTag gibi Entity'den miras almayan sınıfları da sorgulayabileceksin.
        public IQueryable<TRelationalEntity> Query<TRelationalEntity>() where TRelationalEntity : class
        {
            return _db.Set<TRelationalEntity>().AsNoTracking();
        }

        protected void Delete<TRelationalEntity>(List<TRelationalEntity> relationalEntities) where TRelationalEntity : class
        {
            _db.Set<TRelationalEntity>().RemoveRange(relationalEntities);
        }

        // *** RESPONSE METODLARI (ServiceBase olmadığı için buraya ekledik) ***
        // Handler'ların çalışması için bunlar şart.

        public CommandResponse Success(string message = "Operation successful", int id = 0)
        {
            return new CommandResponse(true, message, id);
        }

        public CommandResponse Error(string message = "Operation failed")
        {
            return new CommandResponse(false, message, 0);
        }

        public void Dispose()
        {
            _db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}