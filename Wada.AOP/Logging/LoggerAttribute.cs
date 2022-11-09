using MethodBoundaryAspect.Fody.Attributes;
using NLog;

namespace Wada.AOP.Logging
{
    public sealed class LoggerAttribute : OnMethodBoundaryAspect
    {
        private readonly ILogger logger;

        public LoggerAttribute()
        {
            logger = LogManager.GetCurrentClassLogger();
        }

        public override void OnEntry(MethodExecutionArgs arg)
        {
            //base.OnEntry(arg);
            Console.WriteLine("OnEntry");
            System.Diagnostics.Debug.WriteLine("OnEntry");
            logger.Info("{0} 実行開始", arg.Method.Name);
            arg.MethodExecutionTag = false;
        }

        public override void OnExit(MethodExecutionArgs arg)
        {
            //base.OnExit(arg);
            Console.WriteLine("OnExit");
            System.Diagnostics.Debug.WriteLine("OnExit");
            logger.Info("{0} 実行終了", arg.Method.Name);
            arg.MethodExecutionTag = true;
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            if ((bool)arg.MethodExecutionTag)
                return;

            //base.OnException(arg);
            logger.Error("{0} 例外発生, {1}: {2}", arg.Method.Name, arg.Exception.GetType, arg.Exception.Message);
        }
    }
}
