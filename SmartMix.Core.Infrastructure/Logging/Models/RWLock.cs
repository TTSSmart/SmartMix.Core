namespace TTS.Logger.Models
{
    using System;
    using System.Threading;

    public class RWLock : IDisposable
    {
        public struct WriteLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _locker;
            public WriteLockToken(ReaderWriterLockSlim @lock)
            {
                _locker = @lock;
                @lock.EnterWriteLock();
            }
            public void Dispose() => _locker.ExitWriteLock();
        }

        public struct ReadLockToken : IDisposable
        {
            private readonly ReaderWriterLockSlim _locker;
            public ReadLockToken(ReaderWriterLockSlim @lock)
            {
                _locker = @lock;
                @lock.EnterReadLock();
            }

            #region IDisposable Members

            /// <inheritdoc/>
            public void Dispose()
            {
                // Dispose managed resources
                try
                {
                    _locker.ExitReadLock();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            #endregion IDisposable Members
        }


        private readonly ReaderWriterLockSlim _slimLocker = new ReaderWriterLockSlim();
        public ReadLockToken ReadLock() => new ReadLockToken(_slimLocker);
        public WriteLockToken WriteLock() => new WriteLockToken(_slimLocker);

        #region IDisposable Members

        private bool _disposed;
        /// <inheritdoc/>
        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                try
                {
                    _slimLocker.Dispose();
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            // Free native resources
            _disposed = true;
        }


        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members

        /// <inheritdoc/>
        ~RWLock()
        {
            Dispose(false);
        }

    }
}
