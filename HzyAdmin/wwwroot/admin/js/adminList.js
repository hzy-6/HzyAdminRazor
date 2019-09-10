
/*
 * *******************************************************
 *
 * 作者：hzy
 *
 * 开源地址：https://gitee.com/hao-zhi-ying/
 *
 * *******************************************************
 */
window.adminList = (function () {
    var domButton = {
        panelSearch: function () {
            return $('#panelSearch');
        },
        btnAdd: function () {
            return $('*[name=add]');
        },
        btnEdit: function () {
            return $('*[name=edit]');
        },
        btnDelete: function () {
            return $('*[name=delete]');
        },
        findBack: function () {
            return admin.getQueryString("findback");
        }
    };

    return function (listObject) {
        if (listObject) {
            if (listObject.hasOwnProperty('domButton')) domButton = listObject.domButton;
        }

        var adminListClass = {
            tableObject: {},//表格对象
            tableSelectedIndex: 0,//选中索引
            tableInit: function (tableParameter) {
                var defaultsConfigs = {
                    dom: '#adminTable',
                    url: "",
                    method: "post",
                    idField: "_ukid",
                    //classes:'table table-hover',
                    contentType: "application/x-www-form-urlencoded", //"multipart/form-data",//"application/json",
                    dataType: "json",
                    sidePagination: "server",
                    toolbar: '#toolsButtons',
                    buttonsClass: 'primary',
                    //theadClasses:'thead-light',
                    pageNumber: 1,
                    pageSize: 20,
                    pageList: [15, 20, 50, 100, 1000],
                    showFullscreen: true,
                    pagination: true,
                    clickToSelect: true,
                    maintainMetaData: true,//点击复选框可多选
                    //multipleSelect: true,
                    //ignoreClickToSelectOn: function (e) {
                    //    console.log(e,$(e).hasClass('bs-checkbox'));
                    //    return $(e).hasClass('bs-checkbox');
                    //    //[''].indexOf(e.tagName) > -1
                    //},
                    //singleSelect: true,//单选一行
                    showColumns: true,
                    sortable: true,//是否启用排序
                    silentSort: true,
                    sortStable: true,
                    sortName: null,//定义排序列,通过url方式获取数据填写字段名，否则填写下标
                    sortOrder: "asc",//定义排序方式 'asc' 或者 'desc'
                    //buttonsToolbar: '#tools',
                    height: $(window).height() - 75,
                    data: [],
                    responseHandler: function (res) {
                        return res;
                    },
                    undefinedText: '',
                    queryParamsType: null,
                    queryParams: null,
                    onClickRow: null,
                    onDblClickRow: null,
                    onCheck: null,
                    onCheckAll: null,
                    onLoadSuccess: null,
                };

                //拼接 一下表格的默认 列
                var columns_def = [
                    {
                        field: 'adminListIndex',
                        title: 'No.',
                        width: '35px',
                        align: 'center',
                        formatter: function (value, row, index) {
                            var page = tableObject.bootstrapTable("getOptions");
                            return page.pageSize * (page.pageNumber - 1) + index + 1;
                            //return index + 1;
                        }
                    },
                    {
                        checkbox: true,
                        field: 'adminListState',
                    }
                ];
                if (tableParameter.columns) {
                    for (var i = 0; i < tableParameter.columns.length; i++) {
                        columns_def.push(tableParameter.columns[i]);
                    }
                    tableParameter.columns = columns_def;
                }
                //查看是否有用户自定义参数传入
                for (item in tableParameter) {
                    defaultsConfigs[item] = tableParameter[item];
                }
                //事件处理
                //defaultsConfigs.ajax = function (params) {
                //    if (tableParameter.ajax) {
                //        tableParameter.ajax(params);
                //    }
                //};
                defaultsConfigs.queryParams = function (params) {
                    params = {
                        rows: params.pageSize,   //页面大小
                        page: params.pageNumber,  //页码
                        sortName: params.sortName,  //排序列名
                        sortOrder: params.sortOrder//排位命令（desc，asc）
                    };

                    //将检索的信息放入进去
                    var serialize = domButton.panelSearch().find('form').serialize();
                    if (serialize) {
                        var datas = serialize.split("&");
                        if (datas) {
                            for (var i = 0; i < datas.length; i++) {
                                params[datas[i].split("=")[0]] = decodeURI(datas[i].split("=")[1]);
                            }
                        }
                    }

                    //将地址栏信息组装上
                    var pageUrl = window.location.search;
                    if (pageUrl) {
                        var arrayParams = pageUrl.split('&');
                        for (var i = 0; i < arrayParams.length; i++) {
                            var item = arrayParams[i];
                            var keyValue = item.split('=');
                            params[keyValue[0]] = decodeURI(keyValue[1]);
                        }
                    }

                    if (tableParameter.queryParams) params = tableParameter.queryParams(params);
                    //console.log(params);
                    return params;
                };
                defaultsConfigs.onClickRow = function (row, dom, field) {
                    tableObject.bootstrapTable('uncheckAll');
                    if (tableParameter.onClickRow) {
                        tableParameter.onClickRow(row, dom, field);
                    } else {
                        //adminListClass.tableSelectedIndex = dom.data('index'); //记录选中的id 用来 防止 info页保存后 列表刷新不能记住选中的数据
                        adminListClass.tableSelectedIndex = row._ukid;
                        adminListClass.btnControl();
                    }
                };
                defaultsConfigs.onDblClickRow = function (row, dom, field) {
                    if (tableParameter.onDblClickRow) {
                        tableParameter.onDblClickRow(row, dom, field);
                    } else {
                        if (domButton.findBack()) adminListClass.findBack();
                    }
                };
                defaultsConfigs.onCheck = function (row, dom) {//选中复选框
                    if (tableParameter.onCheck) tableParameter.onCheck(row);
                    adminListClass.btnControl();
                };
                defaultsConfigs.onUncheck = function () {//取消复选框
                    adminListClass.btnControl();
                };
                defaultsConfigs.onCheckAll = function (row) {//选中所有复选框
                    if (tableParameter.onCheckAll) tableParameter.onCheckAll(row);
                    adminListClass.btnControl();
                };
                defaultsConfigs.onUncheckAll = function () {//取消选中所有复选框
                    adminListClass.btnControl();
                };
                defaultsConfigs.onLoadSuccess = function () {
                    //加载完成 检测一下是否有选中的行id 如果有将行 设置为选中状态
                    if (adminListClass.tableSelectedIndex) {
                        tableObject.bootstrapTable("checkBy", { field: "_ukid", values: [adminListClass.tableSelectedIndex] });
                    }
                    if (tableParameter.onLoadSuccess) tableParameter.onLoadSuccess();
                };

                //调用表格插件
                tableObject = $(defaultsConfigs.dom).bootstrapTable(defaultsConfigs);

                // Add responsive   表格自适应
                $(window).bind('resize', function () {
                    tableObject.bootstrapTable('resetView', { height: $(window).height() - 75 });
                });

                this.tableObject = tableObject;
                //end
            },
            //得到 用户选中得行
            selectRows: function () {
                var rows = tableObject.bootstrapTable('getSelections');
                return rows;
            },
            //按钮控制
            btnControl: function () {
                var rows = adminListClass.selectRows();
                if (rows.length > 0) {
                    if (rows.length == 1) {
                        domButton.btnEdit().css('display', 'block');//.removeAttr('disabled');
                        domButton.btnDelete().css('display', 'block');//.removeAttr('disabled');
                    } else {
                        domButton.btnEdit().css('display', 'none');//.attr('disabled', true);
                        domButton.btnDelete().css('display', 'block');//.removeAttr('disabled');
                    }

                } else {
                    domButton.btnEdit().css('display', 'none');//.attr('disabled', true);
                    domButton.btnDelete().css('display', 'none');//.attr('disabled', true);
                }
            },
            //查询面板
            panelSearch: function () {
                domButton.panelSearch().animate({ height: 'toggle' }, 150);//.toggle();
            },
            //重置检索信息
            resetSearch: function () {
                domButton.panelSearch().find("form")[0].reset();
                adminListClass.tableObject.bootstrapTable('selectPage', 1);
            },
            //刷新
            refresh: function (data) {
                //refresh 刷新表格 refreshOptions
                if (data) {
                    adminListClass.tableObject.bootstrapTable('refresh', {
                        query: data
                    });
                } else {
                    adminListClass.tableObject.bootstrapTable('refresh');
                }

                setTimeout(function () {
                    adminListClass.tableObject.bootstrapTable('resetView');
                },300);
                
                //检查一下 按钮控制状态
                setTimeout(function () {
                    adminListClass.btnControl();
                }, 300);
            },
            //打开 表单页
            form: function (options) {
                options.success = function (layero) {
                    top.$(layero).find("iframe").attr("parentFrameName", options.parentFrameName);
                }
                admin.openWindow(options);
            },
            //删除数据
            delete: function (url, callBack) {
                var rows = adminListClass.selectRows();
                if (rows.length == 0) {
                    return admin.msg("请选择要移除的数据");
                }
                admin.confirm("确认删除?", function (index) {
                    var json = [];
                    for (var i = 0; i < rows.length; i++) {
                        json.push(rows[i]._ukid);
                    }
                    admin.ajax({
                        url: url,
                        data: { Ids: JSON.stringify(json) },
                        success: function (r) {
                            if (r.status == 1) {
                                if (callBack) {
                                    callBack();
                                }
                                admin.msg("操作成功！");
                                admin.layer.close(index);
                            }
                        }
                    });
                }, function () {

                });
            },
            //导出excel
            exportExcel: function (url, data) {
                $(event.srcElement).attr("href", url + "?" + domButton.panelSearch().find('form').serialize() + (data ? data : ""));
            },
            //打印
            print: function (url, data) {
                $(event.srcElement).attr("href", url + "?" + domButton.panelSearch().find('form').serialize() + (data ? data : ""));
            },
            findBack: function () {
                var value = domButton.findBack();
                admin.findBack.close(adminListClass.selectRows());
            }
        }

        return adminListClass;
    }

})();