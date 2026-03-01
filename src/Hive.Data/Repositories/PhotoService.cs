using Hive.Core.Models;
using Hive.Core.Services;
using Hive.Data.LocalDb;
using Microsoft.EntityFrameworkCore;

namespace Hive.Data.Repositories;

public class PhotoService : IPhotoService
{
    private readonly HiveDbContext _db;

    public PhotoService(HiveDbContext db) => _db = db;

    public async Task<IReadOnlyList<PhotoAlbum>> GetAlbumsAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.PhotoAlbums
            .Include(a => a.Photos.OrderBy(p => p.SortOrder))
            .Where(a => a.FamilyAccountId == familyAccountId)
            .OrderBy(a => a.Title)
            .ToListAsync(ct);
    }

    public async Task<PhotoAlbum> CreateAlbumAsync(PhotoAlbum album, CancellationToken ct = default)
    {
        album.Id = album.Id == Guid.Empty ? Guid.NewGuid() : album.Id;
        album.CreatedAt = DateTimeOffset.UtcNow;
        _db.PhotoAlbums.Add(album);
        await _db.SaveChangesAsync(ct);
        return album;
    }

    public async Task DeleteAlbumAsync(Guid albumId, CancellationToken ct = default)
    {
        var album = await _db.PhotoAlbums.FindAsync([albumId], ct);
        if (album is not null)
        {
            _db.PhotoAlbums.Remove(album);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<Photo> AddPhotoAsync(Photo photo, CancellationToken ct = default)
    {
        photo.Id = photo.Id == Guid.Empty ? Guid.NewGuid() : photo.Id;
        photo.AddedAt = DateTimeOffset.UtcNow;
        _db.Photos.Add(photo);
        await _db.SaveChangesAsync(ct);
        return photo;
    }

    public async Task DeletePhotoAsync(Guid photoId, CancellationToken ct = default)
    {
        var photo = await _db.Photos.FindAsync([photoId], ct);
        if (photo is not null)
        {
            _db.Photos.Remove(photo);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<Photo>> GetActivePhotosAsync(
        Guid familyAccountId, CancellationToken ct = default)
    {
        return await _db.Photos
            .Include(p => p.Album)
            .Where(p => p.Album != null && p.Album.FamilyAccountId == familyAccountId && p.Album.IsActive)
            .OrderBy(p => p.SortOrder)
            .ToListAsync(ct);
    }
}
