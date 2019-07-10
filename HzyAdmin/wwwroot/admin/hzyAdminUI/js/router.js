/*----------------js路由-------------*/


//路由 对象
var hzyRouter = {
    settings: {
        //路由触发 和加载 事件
        callback: null
    },
    //加载路由
    load: function(text, href) {
        if (href.slice(0, 2) != "#!") {
            href = "#!" + escape(text) + "#!" + href;
        }
        top.window.location.hash = href;
    },
    any: function(text, href) {
        if (href.slice(0, 2) != "#!") {
            href = "#!" + escape(text) + "#!" + href;
        }
        var _hash = top.window.location.hash;
        return _hash.indexOf(href) >= 0;
    },
    //初始化
    init: function(beforeCallback) {
        if (beforeCallback) beforeCallback();
        window.addEventListener('load', function() {
            var hash = top.window.location.hash;
            if (hash.slice(0, 2) != "#!") return;
            var obj = hzyRouter.analysisHash(hash);
            if (hzyRouter.settings.callback) hzyRouter.settings.callback(unescape(obj.text), obj.key, obj.parm, obj.href);
            console.log('load');
        }, false);
        window.addEventListener('hashchange', function() {
            var hash = top.window.location.hash;
            if (hash.slice(0, 2) != "#!") return;
            var obj = hzyRouter.analysisHash(hash);
            if (hzyRouter.settings.callback) hzyRouter.settings.callback(unescape(obj.text), obj.key, obj.parm, obj.href);
        }, false);
    },
    //解析路由
    analysisHash: function(href) {
        //解析路由

        var site = href.split('#!');

        href = "#!" + site[2];

        var text = site[1];

        if (href.indexOf('#!') >= 0) {
            href = href.slice(2) || '/';
        } else {
            href = href || '/';
        }

        var parm = {};

        if (href.indexOf('/?') >= 0) {
            //检测是否有参数 /#!/User/Index/?id=1
            var _index = href.indexOf('/?');
            var _parm = href.substr(_index, href.length - _index).slice(2);

            if (_parm.indexOf('&') > 0) {
                var array = _parm.split('&');
                for (var i = 0; i < array.length; i++) {
                    var _arr = array[i].split('=');
                    parm[_arr[0]] = _arr[1];
                }
            } else {
                var _arr = _parm.split('=');
                parm[_arr[0]] = _arr[1];
            }

            href = href.substring(0, _index);
        }

        return {
            key: href,
            text: text,
            parm: parm,
            href: site[2]
        };
    }
};