using System.Reactive.Subjects;
using Blazored.LocalStorage;

namespace GrowBox.Services;

public class SimpleStorage<TEntity>
    where TEntity : class
{
    private readonly ILocalStorageService _localStorage;
    private readonly Type _entityType;

    public Subject<TEntity?> Subject { get; } = new Subject<TEntity?>();

    public SimpleStorage(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _entityType = typeof(TEntity);
    }

    public async ValueTask<TEntity?> GetAsync(TEntity? defaultEntity = null)
    {        
        try
        {
            var value = await _localStorage.GetItemAsync<TEntity>(_entityType.FullName) ?? defaultEntity;
            Subject.OnNext(value);
            return value;
        }
        catch (Exception)
        {
            Subject.OnNext(defaultEntity);
            return defaultEntity;
        }
    }

    public async ValueTask SetAsync(TEntity? entity)
    {
        await _localStorage.SetItemAsync(_entityType.FullName, entity);
        Subject.OnNext(entity);
    }
    
}
public class SimpleStorage<TEntity, TKey>
    where TEntity : class
{
    private readonly ILocalStorageService _localStorage;
    private readonly Type _keyType;

    public Subject<TEntity?> Subject { get; } = new Subject<TEntity?>();

    public SimpleStorage(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _keyType = typeof(TKey);
    }

    public async ValueTask<TEntity?> GetAsync(TEntity? defaultEntity = null)
    {        
        try
        {
            var value = await _localStorage.GetItemAsync<TEntity>(_keyType.FullName) ?? defaultEntity;
            Subject.OnNext(value);
            return value;
        }
        catch (Exception)
        {
            Subject.OnNext(defaultEntity);
            return defaultEntity;
        }
    }

    public async ValueTask SetAsync(TEntity? entity)
    {
        await _localStorage.SetItemAsync(_keyType.FullName, entity);
        Subject.OnNext(entity);
    }
}
