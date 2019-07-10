using System;

namespace Toolkit
{
    /// <summary>
    /// 配合Web页面使用纯ajax调用函数，后端弹出提示框的 类   AJAX 全局拦截消息
    /// </summary>
    [Serializable]
    public class MessageBox : Exception
    {
        public MessageBox(string Messager)
            : base(Messager)
        {
            ErrorModel.Model = new ErrorModel(Messager);
        }

        public MessageBox(object Messager)
            : base($"自定义异常 请忽略! Data>>>{Messager}")
        {
            ErrorModel.Model = new ErrorModel(Messager, EMsgStatus.Custom60);
        }

        public MessageBox(ErrorModel _ErrorModel)
            : base($"自定义异常 请忽略! Data>>>{_ErrorModel.msg}")
        {
            ErrorModel.Model = _ErrorModel;
        }

        public MessageBox(string Messager, EMsgStatus msgstatus)
            : base(Messager)
        {
            ErrorModel.Model = new ErrorModel(Messager, msgstatus);
        }

        public MessageBox(string Messager, Exception InnerException)
            : base(Messager, InnerException)
        {
            ErrorModel.Model = new ErrorModel(Messager);
        }

    }

    public class ErrorModel
    {
        /// <summary>
        /// 异常模型
        /// </summary>
        public static ErrorModel Model { set; get; }

        public ErrorModel()
        {

        }

        public ErrorModel(string msg)
        {
            this.EM(msg, EMsgStatus.信息提示10);
        }

        public ErrorModel(object msg, EMsgStatus msgStatus)
        {
            if (msgStatus == EMsgStatus.Custom60)
            {
                this.Data = msg;
                this.status = (int)msgStatus;
            }
            else
                this.EM(msg.ToString(), msgStatus);
        }

        public ErrorModel(Exception exception)
        {
            this.msg = exception.Message;
            this.status = (int)EMsgStatus.程序异常50;
            this.StackTrace = exception.StackTrace;
            this.TargetSite = exception.TargetSite != null ? exception.TargetSite.Name : null;
            this.Source = exception.Source;
            this.Data = exception.Data != null ? exception.Data.ToString() : "";
        }

        private void EM(string msg, EMsgStatus msgStatus)
        {
            this.msg = msg;
            this.status = (int)msgStatus;
        }

        /// <summary>
        /// 错误状态码
        /// </summary>
        public int status { set; get; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string msg { set; get; }

        /// <summary>
        /// 跳转地址
        /// </summary>
        public string jumpUrl { get; set; }

        /// <summary>
        /// 堆栈
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// 异常引发方法
        /// </summary>
        public string TargetSite { get; set; }

        /// <summary>
        /// 异常对象或应用程序
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 异常信息键值对集合
        /// </summary>
        public object Data { get; set; }

    }

    /// <summary>
    /// 消息状态
    /// </summary>
    public enum EMsgStatus
    {
        信息提示10 = 10,
        登录超时20 = 20,
        手动处理30 = 30,
        程序异常50 = 50,
        Custom60 = 60
    }


}
