using System.Text;
using NLog;
using NLog.LayoutRenderers;
using Raven.Imports.Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    [LayoutRenderer("json-renderer")]
    public class JsonLayoutRenderer : LayoutRenderer
    {
        public bool RenderException { get; set; }
        public bool RenderParameters { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            if (RenderParameters)
                AppendParameters(builder, logEvent);
            if (RenderException)
                AppendException(builder, logEvent);

        }

        private void AppendParameters(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Parameters != null && logEvent.Parameters.Length > 0)
            {
                builder.Append(logEvent.Parameters.Length == 1
                    ? JsonConvert.SerializeObject(logEvent.Parameters[0])
                    : JsonConvert.SerializeObject(logEvent.Parameters));
            }
        }

        private void AppendException(StringBuilder builder, LogEventInfo logEvent)
        {
            if (logEvent.Exception != null)
                builder.Append(JsonConvert.SerializeObject(logEvent.Exception));
        }
    }
}