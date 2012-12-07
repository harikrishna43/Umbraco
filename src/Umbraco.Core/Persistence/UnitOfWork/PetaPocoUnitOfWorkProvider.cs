namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="PetaPocoUnitOfWork"/>
    /// </summary>
    internal class PetaPocoUnitOfWorkProvider : IUnitOfWorkProvider
    {
        #region Implementation of IUnitOfWorkProvider

        public IUnitOfWork GetUnitOfWork()
        {
            return new PetaPocoUnitOfWork();
        }

        #endregion
    }
}