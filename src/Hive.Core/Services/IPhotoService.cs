using Hive.Core.Models;

namespace Hive.Core.Services;

public interface IPhotoService
{
    Task<IReadOnlyList<PhotoAlbum>> GetAlbumsAsync(Guid familyAccountId, CancellationToken ct = default);
    Task<PhotoAlbum> CreateAlbumAsync(PhotoAlbum album, CancellationToken ct = default);
    Task DeleteAlbumAsync(Guid albumId, CancellationToken ct = default);
    Task<Photo> AddPhotoAsync(Photo photo, CancellationToken ct = default);
    Task DeletePhotoAsync(Guid photoId, CancellationToken ct = default);
    Task<IReadOnlyList<Photo>> GetActivePhotosAsync(Guid familyAccountId, CancellationToken ct = default);
}
