/*
 * *******************************************************
 * 
 * 作者：hzy
 * 
 * 开源地址：https://gitee.com/hao-zhi-ying
 * 
 * *******************************************************
 */
var hzy = {
    settings: {
        isIframe: false, //如果启用则使用iframe选项卡 , 否在就是单页面
        //页面加载后放入的节点
        mainContainer: '.hzy-container',
        pageContainer: '.hzy-main-right content',
        tabsContainer: '.hzy-main-right .hzy-center-menu ul',
        menuContainer: 'main nav.hzy-menu',
        menuSessionName: 'adminMW',
        menuSkinSessionName: 'adminMenuSkin'
    },
    header: {
        keyName: 'headerSkin',
        init: function() {
            var skin = window.localStorage.getItem(hzy.header.keyName);
            if (skin) $('.hzy-container header').removeAttr('class').addClass(skin);
        },
        setSkin: function(skin) {
            $('.hzy-container header').removeAttr('class').addClass(skin);
            window.localStorage.setItem(hzy.header.keyName, skin);
        }
    },
    init: function() {
        //路由事件 触发回调
        hzyRouter.settings.callback = function(t, k, p, h) {
            //t:标题 h:链接地址 p:参数
            //console.log('t=', t, 'k=', k, 'p=', p, 'h', h)
            var hash = top.window.location.hash;
            var arry = hash.split("#!");
            hzy.tabs.add({
                href: arry[2],
                text: t
            });
        };
        //给有 hzy-router= 特性的标签添加click
        $(hzy.settings.mainContainer).on('click', '[hzy-router]', function() {
            var _routers = $(this).attr("hzy-router");
            var _router = _routers.split('#!');
            hzy.jumpPage(_router[1], _router[2]);
        });

        hzy.header.init();
        hzy.tabs.init();
        hzy.menu.init();
        window.onresize = function() {
            hzy.resize();
        };
    },
    resize: function() {
        $(hzy.settings.tabsContainer).offset({
            left: hzy.tabs.initUlOffsetLeft
        });
        hzy.tabs.setTabDomInit();
        hzy.menu.init();
    },
    isPc: function() {
        return window.innerWidth > 990;
    },
    //跳转页面
    jumpPage: function(text, href) {
        hzyRouter.load(text, href);
    },
    //加载页面
    loadPage: function(href, callback) {
        var _pageContainer = $(hzy.settings.pageContainer);
        var _temp = `<iframe class="hzy-iframe hzy-iframe-active" frameborder="0" src="` + href + `" name="` + href + `"></iframe>`;
        var _loadHtml = `
<div class="example-loading example-well vertical-align text-center hzy-page-loader">
<div class="loader vertical-align-middle loader-cube-grid"></div></div>`;
        //如果是iframe 模式
        if (hzy.settings.isIframe) {
            _pageContainer.find('iframe').removeClass('hzy-iframe-active');
            var _iframe = _pageContainer.find('iframe[src="' + href + '"]');
            if (_iframe.length == 0) {
                _pageContainer.prepend(_loadHtml);
                _pageContainer.append(_temp);
                _iframe = _pageContainer.find('iframe[src="' + href + '"]');
                hzy.handleIframeLoadSuccess(_iframe[0], function() {
                    setTimeout(function() {
                        _pageContainer.find('.hzy-page-loader').remove();
                    }, 300);
                });
            } else
                _iframe.addClass('hzy-iframe-active');
            if (callback) callback(_iframe);
            return;
        }
        //如果是非iframe 模式
        _pageContainer.empty();
        if (href.indexOf('http://') >= 0 || href.indexOf('https://') >= 0) {
            _pageContainer.prepend(_loadHtml);
            _pageContainer.append(_temp);
            var _iframe = _pageContainer.find('iframe[src="' + href + '"]');
            hzy.handleIframeLoadSuccess(_iframe[0], function() {
                setTimeout(function() {
                    _pageContainer.find('.hzy-page-loader').remove();
                }, 300);
            });
            if (callback) callback();
            return;
        }
        //
        if (href.indexOf('?') >= 0) {
            href += '&pt=tabs';
        } else {
            href += '?pt=tabs';
        }
        _pageContainer.prepend(_loadHtml);
        _pageContainer.load(href, function(response, status, xhr) {
            //if (status === 'success') console.log('页面' + href + '加载成功!');
            if (callback) callback();
            setTimeout(function() {
                _pageContainer.find('.hzy-page-loader').remove();
            }, 300);
        });

    },
    //菜单管理
    menu: {
        domObject: null, //菜单初始化对象
        mMaxWidth: 'sidebar-nav-max', //菜单最大宽度
        mMinWidth: 'sidebar-nav-min', //菜单最小宽度
        mSkin: {
            //菜单皮肤
            default: 'sidebar-nav',
            white: 'sidebar-nav-white'
        },
        init: function() {
            var _menu = $(hzy.settings.menuContainer);
            if (!hzy.isPc()) {
                if (_menu.hasClass(hzy.menu.mMaxWidth)) {
                    _menu.removeClass(hzy.menu.mMaxWidth);
                    _menu.addClass(hzy.menu.mMinWidth);
                }
            } else {
                var w = window.localStorage.getItem(hzy.settings.menuSessionName);
                if (w) {
                    _menu.removeClass(hzy.menu.mMaxWidth).removeClass(hzy.menu.mMinWidth).addClass(w);
                }
                var skin = window.localStorage.getItem(hzy.settings.menuSkinSessionName);
                if (skin) {
                    hzy.menu.setSkin(skin);
                }
            }
        },
        //激活菜单 选中状态
        active: function(text, href) {
            //激活左侧菜单
            // hzy-router="#!首页2#!views/home2.html"
            if (!hzy.menu.domObject) return;
            hzy.menu.domObject.find('li').removeClass('mm-active').find('a').removeAttr('aria-expanded');
            var li = hzy.menu.domObject.find('[hzy-router="#!' + text + '#!' + href + '"]');
            li.addClass('mm-active').find('a').attr('aria-expanded', true);
            var w = window.localStorage.getItem(hzy.settings.menuSessionName);
            if (w === hzy.menu.mMinWidth) {
                li.parents('li').addClass('mm-active');
                hzy.menu.domObject.find('ul').removeClass('mm-show');
            } else {
                if (!li.parents('li').hasClass('mm-active')) li.parents('li').find('a.has-arrow:eq(0)').click();
            }
            hzy.tabs.location();
        },
        toggle: function() {
            var _menuContainer = $(hzy.settings.menuContainer);
            if (_menuContainer.hasClass(hzy.menu.mMaxWidth))
                hzy.menu.min(_menuContainer);
            else
                hzy.menu.max(_menuContainer);
        },
        max: function(_menuContainer) {
            _menuContainer.addClass(hzy.menu.mMaxWidth);
            _menuContainer.removeClass(hzy.menu.mMinWidth);
            if (hzy.isPc())
                window.localStorage.setItem(hzy.settings.menuSessionName, hzy.menu.mMaxWidth);
            else
                window.localStorage.removeItem(hzy.settings.menuSessionName);
        },
        min: function(_menuContainer) {
            _menuContainer.addClass(hzy.menu.mMinWidth);
            _menuContainer.removeClass(hzy.menu.mMaxWidth);
            if (hzy.isPc())
                window.localStorage.setItem(hzy.settings.menuSessionName, hzy.menu.mMinWidth);
            else
                window.localStorage.removeItem(hzy.settings.menuSessionName);
        },
        setSkin: function(skin) {
            var _menu = $(hzy.settings.menuContainer);
            _menu.removeClass(hzy.menu.mSkin.default).removeClass(hzy.menu.mSkin.white);
            _menu.addClass(skin);
            window.localStorage.setItem(hzy.settings.menuSkinSessionName, skin);
        }
    },
    //选项卡操作
    tabs: {
        visualAreaWidth: 0, //选项卡可见区域
        ulTotalWidth: 0, //ul 宽度
        liTotalWidth: 0, //li 宽度
        initUlOffsetLeft: 0, //ul 第一次偏移量
        leftAndRightLock: true, //对点击左移动和右移动加锁
        moveSpeed: 300, //选项卡移动延迟
        init: function() {
            hzy.tabs.setTabDomInit();
            hzy.tabs.initUlOffsetLeft = $(hzy.settings.tabsContainer).offset().left;
            //监听鼠标滚轮事件
            $(hzy.settings.tabsContainer).parent().mousewheel(function(event) {
                if (event.deltaY === 1)
                    hzy.tabs.goRight();
                else
                    hzy.tabs.goLeft();
                // console.log(event.deltaX, event.deltaY, event.deltaFactor);
            });
        },
        setTabDomInit: function() {
            //设置选项卡dom元素初始值
            hzy.tabs.liTotalWidth = $(hzy.settings.tabsContainer + ' li:eq(0)').width();
            var _w = hzy.tabs.liTotalWidth * $(hzy.settings.tabsContainer + ' li').length;
            $(hzy.settings.tabsContainer).width(_w);
            hzy.tabs.ulTotalWidth = _w;
            hzy.tabs.visualAreaWidth = $(hzy.settings.tabsContainer).parent().width();
        },
        //添加一个选项
        add: function(obj) {
            var _href = obj.href,
                _text = obj.text;
            var _checkDom = $(hzy.settings.tabsContainer + ' li[hzy-router="#!' + _text + '#!' + _href + '"]');
            $(hzy.settings.tabsContainer + ' li').removeClass('selected');
            if (_checkDom.length > 0) { //如果存在
                _checkDom.addClass('selected');
            } else {
                var _temp = `<li class="selected" hzy-router="#!` + _text + `#!` + _href + `"><span>` + _text + `</span><i class="far fa-times-circle" onclick="hzy.tabs.close(this);"></i></li>`;
                $(hzy.settings.tabsContainer).append(_temp);
            }
            hzy.tabs.setTabDomInit();
            hzy.tabs.location();
            //加载页面
            hzy.loadPage(_href, function() {
                hzy.menu.active(_text, _href);
            });
        },
        goLeft: function(e) {
            hzy.tabs.checkMove(function(_ul, moveVal) { //偏移量                
                var _lastLi = _ul.find('li:last');
                var _checkVal = (hzy.tabs.initUlOffsetLeft + hzy.tabs.visualAreaWidth);
                if (_lastLi.offset().left <= (_checkVal - hzy.tabs.liTotalWidth)) return;
                hzy.tabs.move(moveVal, '-');
            });
        },
        goRight: function(e) {
            hzy.tabs.checkMove(function(_ul, moveVal) { //偏移量
                if (_ul.offset().left >= hzy.tabs.initUlOffsetLeft) return;
                hzy.tabs.move(moveVal, '+');
            });
        },
        checkLeftAndRight: function(_ul, now, operator, callback) {
            if (operator === '+' && now >= 0) { //如果是右移动
                if (callback) callback();
                _ul.stop();
                return;
            }
            var _lastLi = _ul.find('li:last');
            var _checkVal = (hzy.tabs.initUlOffsetLeft + hzy.tabs.visualAreaWidth);
            if (operator === '-' && _lastLi.offset().left <= (_checkVal - hzy.tabs.liTotalWidth)) {
                _ul.stop();
                return;
            }
        },
        checkMove: function(callback) {
            // 检查能否移动
            if (!hzy.tabs.leftAndRightLock) return;
            if (hzy.tabs.ulTotalWidth <= hzy.tabs.visualAreaWidth) return;
            var _ul = $(hzy.settings.tabsContainer);
            var _valli = hzy.tabs.liTotalWidth * 4;
            if (callback) callback(_ul, _valli);
        },
        move: function(val, operator) {
            //移动
            var _ul = $(hzy.settings.tabsContainer);
            hzy.tabs.leftAndRightLock = false;
            var _record = false;
            _ul.animate({ "left": operator + '=' + val + "px" }, {
                duration: hzy.tabs.moveSpeed,
                easing: 'swing',
                step: function(now, fx) {
                    //操作每一帧
                    hzy.tabs.checkLeftAndRight(_ul, now, operator, function() {
                        _record = true;
                    });
                },
                always: function() {
                    if (_record) {
                        //改邪归正
                        setTimeout(function() {
                            _ul.animate({ 'left': '0' }, 'normal');
                        }, 5);
                    }
                    hzy.tabs.leftAndRightLock = true;
                }
            });
        },
        location: function() {
            //智能定位
            hzy.tabs.checkMove(function(_ul, _valli) {
                var _activeLiOffsetLeft = _ul.find('li.selected').offset().left;
                var _checkVal = (hzy.tabs.initUlOffsetLeft + hzy.tabs.visualAreaWidth); //可视区域偏移量
                //如果菜单隐藏在右边 那么向左移动
                if (_activeLiOffsetLeft > (_checkVal - _valli)) {
                    var _offset = Math.abs((_activeLiOffsetLeft + _valli) - _checkVal);
                    hzy.tabs.move(_offset, '-');
                }

                //如果菜单隐藏在左边 那么向右移动
                if (_activeLiOffsetLeft < hzy.tabs.initUlOffsetLeft) {
                    var _offset = Math.abs(hzy.tabs.initUlOffsetLeft - _activeLiOffsetLeft);
                    hzy.tabs.move(_offset, '+');
                }
            });
        },
        close: function(_this) {
            //关闭
            _this = $(_this).parent();
            var obj = hzyRouter.analysisHash(_this.attr('hzy-router'));
            _this.prev().click();
            _this.remove();
            if (hzy.settings.isIframe) { //如果iframe 模式
                $(hzy.settings.pageContainer).find('iframe[src="' + obj.href + '"]').remove();
            }
        },
        closeOther: function() {
            //关闭其他
            var lis = $(hzy.settings.tabsContainer).find('li:gt(0)');
            lis.each(function() {
                if (!$(this).hasClass('selected')) {
                    var obj = hzyRouter.analysisHash($(this).attr('hzy-router'));
                    if (hzy.settings.isIframe) { //如果iframe 模式
                        $(hzy.settings.pageContainer).find('iframe[src="' + obj.href + '"]').remove();
                    }
                    $(this).remove();
                }
            });
        },
        closeAll: function() {
            //关闭所有
            var lis = $(hzy.settings.tabsContainer).find('li:gt(0)');
            lis.each(function() {
                var obj = hzyRouter.analysisHash($(this).attr('hzy-router'));
                if (hzy.settings.isIframe) { //如果iframe 模式
                    $(hzy.settings.pageContainer).find('iframe[src="' + obj.href + '"]').remove();
                }
            });
            //
            lis.remove();
            $(hzy.settings.tabsContainer).find('li:eq(0)').click();
        },
        refreshActive: function(_this) {
            //刷新激活的选项卡
            var hash = top.window.location.hash;
            var arry = hash.split("#!");
            hzy.loadPage(arry[2], function(_iframe) {
                if (hzy.settings.isIframe) _iframe.attr('src', arry[2]);
            });
        },
    },
    //监听iframe 对象加载完成
    handleIframeLoadSuccess: function(iframe, callBack) {
        if (iframe.attachEvent) {
            //todo something
            callBack();
        } else {
            iframe.onload = function() {
                //todo something
                callBack();
            }
        }
    },
    //全屏 类
    fullScreen: function() {
        var isFullScreen = false;
        var requestFullScreen = function() { //全屏
            var de = document.documentElement;
            if (de.requestFullscreen) {
                de.requestFullscreen();
            } else if (de.mozRequestFullScreen) {
                de.mozRequestFullScreen();
            } else if (de.webkitRequestFullScreen) {
                de.webkitRequestFullScreen();
            } else {
                alert("该浏览器不支持全屏");
            }
        };
        //退出全屏 判断浏览器种类
        var exitFull = function() {
            // 判断各种浏览器，找到正确的方法
            var exitMethod = document.exitFullscreen || //W3C
                document.mozCancelFullScreen || //Chrome等
                document.webkitExitFullscreen || //FireFox
                document.webkitExitFullscreen; //IE11
            if (exitMethod) {
                exitMethod.call(document);
            } else if (typeof window.ActiveXObject !== "undefined") { //for Internet Explorer
                var wscript = new ActiveXObject("WScript.Shell");
                if (wscript !== null) {
                    wscript.SendKeys("{F11}");
                }
            }
        };

        return {
            handleFullScreen: function($this) {
                $this = $($this);
                if (isFullScreen) {
                    exitFull();
                    isFullScreen = false;
                    $this.find("i").removeClass("fas fa-compress");
                    $this.find("i").addClass("fas fa-expand");
                } else {
                    requestFullScreen();
                    isFullScreen = true;
                    $this.find("i").removeClass("fas fa-expand");
                    $this.find("i").addClass("fas fa-compress");
                }
            },
        };

    }()
};