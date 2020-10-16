﻿var $url = '/settings/administratorsLayerPassword';
var $pageTypeAdmin = 'admin';
var $pageTypeUser = 'user';

var data = utils.init({
  pageType: utils.getQueryString('pageType'),
  userId: utils.getQueryInt('userId'),
  administrator: null,
  form: {
    password: null,
    confirmPassword: null
  }
});

var methods = {
  apiGet: function () {
    var $this = this;

    $api.get($url, {
      params: {
        userId: this.userId
      }
    }).then(function (response) {
      var res = response.data;

      $this.administrator = res.administrator;

      if (!$this.pageType && $this.userId === 0) {
        this.$message({
          type: 'warning',
          message: '您的密码已过期，请更改登录密码',
          showClose: true,
          duration: 0
        });
      }
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  apiSubmit: function () {
    var $this = this;

    utils.loading(this, true);
    $api.post($url, {
      userId: this.userId,
      password: this.form.password
    }).then(function (response) {
      var res = response.data;

      utils.success('密码更改成功！');

      setTimeout(function () {
        utils.closeLayer();
      }, 1000);
    }).catch(function (error) {
      utils.error(error);
    }).then(function () {
      utils.loading($this, false);
    });
  },

  validatePass: function(rule, value, callback) {
    if (value === '') {
      callback(new Error('请再次输入密码'));
    } else if (value !== this.form.password) {
      callback(new Error('两次输入密码不一致!'));
    } else {
      callback();
    }
  },

  btnSubmitClick: function() {
    var $this = this;

    this.$refs.form.validate(function(valid) {
      if (valid) {
        $this.apiSubmit();
      }
    });
  },

  btnCancelClick: function () {
    utils.closeLayer();
  }
};

var $vue = new Vue({
  el: '#main',
  data: data,
  methods: methods,
  created: function () {
    this.apiGet();
  }
});