using StatCore.Stats;

namespace StatCore.DataFlow
{
    public class StatConnecter<TIn, TOut, TStat> : IStat<TIn, TStat>
    {
        private IStat<TOut, TStat> stat;
        private IConnectableStat<TIn, TOut> connection;

        public StatConnecter(IConnectableStat<TIn, TOut> connection, IStat<TOut, TStat> stat)
        {
            this.connection = connection;
            this.stat = stat;
            connection.Added += (_, item) => stat.Add(item);
            connection.Deleted += (_, item) => stat.Delete(item);
        }

        public void Add(TIn item)
        {
            connection.Add(item);
        }

        public void Delete(TIn item)
        {
            connection.Delete(item);
        }

        public TStat Value => stat.Value;
        public bool IsEmpty => stat.IsEmpty;
    }
}