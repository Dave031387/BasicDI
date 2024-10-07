namespace BasicDI;

/// <summary>
/// The <see cref="Scope" /> class is used manage the creation and lifetime of scoped dependencies.
/// </summary>
internal class Scope : IScope, IDisposable
{
    /// <summary>
    /// Hold a reference to the dependency injection container.
    /// </summary>
    internal readonly Container _container;

    /// <summary>
    /// Flag to detect redundant calls to the <see cref="Dispose(bool)" /> method.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// Create a new instance of the <see cref="Scope" /> class.
    /// </summary>
    internal Scope(Container container)
    {
        _container = container;
        Guid = Guid.NewGuid();
    }

    /// <summary>
    /// Gets the <see cref="System.Guid" /> value that uniquely identifies this scope.
    /// </summary>
    public Guid Guid
    {
        get;
        private set;
    }

    /// <summary>
    /// Dispose of the managed resources that are owned by this scope.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose of the managed resources that are owned by this scope and then set a flag to prevent
    /// redundant calls to this method.
    /// </summary>
    /// <param name="disposing">
    /// A boolean flag indicating whether or not managed resources should be disposed of.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        if (disposing)
        {
            // Free managed resources here
        }

        // Unmanaged resources would be freed here if there were any.

        _isDisposed = true;
    }
}