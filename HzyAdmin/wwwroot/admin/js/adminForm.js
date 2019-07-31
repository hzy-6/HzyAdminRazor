/*
 * *******************************************************
 *
 * 作者：hzy
 *
 * 开源地址：https://gitee.com/hao-zhi-ying/
 *
 * *******************************************************
 */

window.adminForm = (function () {

    //vueObject=vue js 对象、ignoreDataObject=存放忽略data 字段的 数组 、 callBack(winDom)=用于操作窗口标题
    return function (vueObject, ignoreDataObject, callBack) {

        var thisFrameIndex = admin.layer.getFrameIndex(window.name);
        var addName = "添加";
        var updateName = "修改";
        var successMessage = "操作成功!";

        var adminFormClass = {
            //初始化
            init: function (options) {
                var defaults = {
                    formKey: null,
                    url: null,
                    data: null,
                    success: null,
                    callBack: null
                };
                var options = $.extend({}, defaults, options);

                if (callBack)
                    callBack();
                else
                    adminFormClass.setWindowTitle((options.formKey ? updateName : addName));

                //请求
                admin.ajax({
                    url: options.url,
                    data: options.data ? options.data : { ID: options.formKey },
                    loading: true,
                    success: function (r) {
                        if (options.success) return options.success(r);
                        if (r.status == 1) {
                            admin.vueDataMapping(r, vueObject);
                            if (options.callBack) options.callBack(r);
                        }
                    }
                });

            },
            //保存
            save: function (options) {
                var defaults = {
                    url: "",
                    data: null,
                    success: null,
                    callBack: null,
                    msg: null,
                    isupfile: false,//是否有文件上传
                    isClose: false,//保存完成是否关闭窗口
                };
                var options = $.extend({}, defaults, options);
                if (!options.url) return;

                var _data = options.data;
                if (!_data) {
                    _data = {};
                    for (item in vueObject.$data) {
                        _data[item] = vueObject.$data[item];
                    }
                    for (var i = 0; i < ignoreDataObject.length; i++) {
                        var item = ignoreDataObject[i];
                        delete _data[item];
                    }

                }

                //请求
                admin.ajax({
                    url: options.url,
                    data: _data,
                    isupfile: options.isupfile,
                    loading: true,
                    success: function (r) {
                        if (r.hasOwnProperty('formKey')) {
                            vueObject.formKey = r.formKey;
                            vueObject.load();
                        }
                        if (options.success) return options.success(r);
                        if (r.status == 1) {
                            admin.msg(options.msg ? options.msg : successMessage, "成功");
                            if (options.callBack) options.callBack(r);
                            if (options.isClose) adminFormClass.closeWindow();
                        }
                    }
                });


            },
            //创建 FormData 一般用于上传文件 组装数据使用该函数
            createFormData: function () {
                var _FormData = new FormData();
                var vueData = vueObject.$data;
                for (item in vueData) {
                    _FormData.append(item, vueData[item]);
                }

                for (var i = 0; i < ignoreDataObject.length; i++) {
                    var item = ignoreDataObject[i];
                    _FormData.delete(item);
                }

                return _FormData;
            },
            //设置窗口标题
            setWindowTitle: function (title) {
                var _winDom = top.$("#layui-layer" + thisFrameIndex + ">.layui-layer-title");
                _winDom.text(title);
                return _winDom;
            },
            //关闭当前窗口
            closeWindow: function () {
                admin.layer.close(thisFrameIndex);
            },
            //刷新父级
            refreshParent: function (parentName, callBack) {
                if (!parentName) parentName = top.$('content iframe.hzy-iframe-active').attr('name');
                if (!parentName) parentName = admin.getParentFrameName();
                if (!parentName) alert("找不到需要刷新父级iframe对象!");
                var _parentFrame = top.window.frames[parentName];
                if (callBack) return callBack(_parentFrame);
                _parentFrame.app.refresh();
            }

        };

        return adminFormClass;

    }
})();

