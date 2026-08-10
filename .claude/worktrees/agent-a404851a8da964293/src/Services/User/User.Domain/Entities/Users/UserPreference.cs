namespace NovaCore.User.Domain.Entities.Users;

/// <summary>
/// Owned 1:1 extension of User holding shopping-personalization signals. RecentlyViewedProducts
/// and SearchHistory are capped, most-recent-first lists - unbounded growth would turn this row
/// into an ever-growing log rather than a personalization signal.
/// </summary>
public sealed class UserPreference : BaseEntity, IAuditable, ITenantEntity
{
    private const int MaxRecentlyViewedProducts = 50;
    private const int MaxSearchHistoryEntries = 50;

    private readonly List<Guid> _favoriteCategories = [];
    private readonly List<Guid> _favoriteBrands = [];
    private readonly List<Guid> _recentlyViewedProducts = [];
    private readonly List<string> _searchHistory = [];

    public Guid UserId { get; private set; }
    public User User { get; private set; } = default!;
    public IReadOnlyCollection<Guid> FavoriteCategories => _favoriteCategories;
    public IReadOnlyCollection<Guid> FavoriteBrands => _favoriteBrands;
    public string? PreferredWarehouseCode { get; private set; }
    public IReadOnlyCollection<Guid> RecentlyViewedProducts => _recentlyViewedProducts;
    public IReadOnlyCollection<string> SearchHistory => _searchHistory;

    public Guid TenantId { get; private set; }

    public void AssignTenant(Guid tenantId)
    {
        if (TenantId == Guid.Empty)
            TenantId = tenantId;
    }

    private UserPreference() { }

    public static UserPreference Create(Guid userId, string? preferredWarehouseCode = null)
    {
        return new UserPreference
        {
            UserId = userId,
            PreferredWarehouseCode = preferredWarehouseCode,
        };
    }

    // ============================================================================
    // Favorite categories & brands
    // Manages the FavoriteCategories/FavoriteBrands sets used to bias
    // recommendations and merchandising. Both are plain sets - no duplicates,
    // order does not matter.
    // ============================================================================

    #region Favorite categories & brands

    public void AddFavoriteCategory(Guid categoryId)
    {
        if (!_favoriteCategories.Contains(categoryId))
            _favoriteCategories.Add(categoryId);
    }

    public void RemoveFavoriteCategory(Guid categoryId)
    {
        _favoriteCategories.Remove(categoryId);
    }

    public void AddFavoriteBrand(Guid brandId)
    {
        if (!_favoriteBrands.Contains(brandId))
            _favoriteBrands.Add(brandId);
    }

    public void RemoveFavoriteBrand(Guid brandId)
    {
        _favoriteBrands.Remove(brandId);
    }

    #endregion

    // ============================================================================
    // Recently viewed & search history
    // Manages the two capped, most-recent-first activity lists. Re-viewing or
    // re-searching an existing entry moves it back to the front rather than
    // creating a duplicate.
    // ============================================================================

    #region Recently viewed & search history

    public void RecordProductView(Guid productId)
    {
        _recentlyViewedProducts.Remove(productId);
        _recentlyViewedProducts.Insert(0, productId);

        if (_recentlyViewedProducts.Count > MaxRecentlyViewedProducts)
            _recentlyViewedProducts.RemoveRange(MaxRecentlyViewedProducts, _recentlyViewedProducts.Count - MaxRecentlyViewedProducts);
    }

    public void ClearRecentlyViewedProducts()
    {
        _recentlyViewedProducts.Clear();
    }

    public void RecordSearchTerm(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return;

        var normalized = term.Trim();
        _searchHistory.RemoveAll(existing => string.Equals(existing, normalized, StringComparison.OrdinalIgnoreCase));
        _searchHistory.Insert(0, normalized);

        if (_searchHistory.Count > MaxSearchHistoryEntries)
            _searchHistory.RemoveRange(MaxSearchHistoryEntries, _searchHistory.Count - MaxSearchHistoryEntries);
    }

    public void ClearSearchHistory()
    {
        _searchHistory.Clear();
    }

    #endregion

    // ============================================================================
    // Details & lifecycle
    // The preferred fulfillment warehouse used to bias stock/ETA display.
    // ============================================================================

    #region Details & lifecycle

    public void SetPreferredWarehouse(string? preferredWarehouseCode)
    {
        PreferredWarehouseCode = preferredWarehouseCode;
    }

    #endregion
}
