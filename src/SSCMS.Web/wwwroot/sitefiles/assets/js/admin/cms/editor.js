﻿var $url = '/cms/editor/editor';

var data = utils.init({
  siteId: utils.getQueryInt('siteId'),
  channelId: utils.getQueryInt('channelId'),
  contentId: utils.getQueryInt('contentId'),
  page: utils.getQueryInt('page'),
  tabName: utils.getQueryString('tabName'),
  mainHeight: '',
  isSettings: true,
  sideType: 'first',
  collapseSettings: ['checkedLevel', 'addDate'],
  collapseMore: ['translates'],

  site: null,
  siteUrl: null,
  channel: null,
  groupNames: null,
  tagNames: null,
  checkedLevels: null,
  siteOptions: null,
  channelOptions: null,
  styles: null,
  form: null,

  translates: [],
  isPreviewSaving: false
});

var methods = {
  runFormLayerImageUploadText: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runFormLayerImageUploadEditor: function(attributeName, html) {
    this.insertEditor(attributeName, html);
  },

  runMaterialLayerImageSelect: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runFormLayerFileUpload: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runMaterialLayerFileSelect: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runFormLayerVideoUpload: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runMaterialLayerVideoSelect: function(attributeName, no, text) {
    this.insertText(attributeName, no, text);
  },

  runEditorLayerImage: function(attributeName, html) {
    this.insertEditor(attributeName, html);
  },

  insertText: function(attributeName, no, text) {
    var count = this.form[utils.getCountName(attributeName)];
    if (count && count < no) {
      this.form[utils.getCountName(attributeName)] = no;
    }
    this.form[utils.getExtendName(attributeName, no)] = text;
    this.form = _.assign({}, this.form);
  },

  insertEditor: function(attributeName, html) {
    if (!attributeName) attributeName = 'Body';
    if (!html) return;
    UE.getEditor(attributeName, {allowDivTransToP: false, maximumWords:99999999}).execCommand('insertHTML', html);
  },

  addTranslation: function(targetSiteId, targetChannelId, translateType, summary) {
    this.translates.push({
      siteId: this.siteId,
      channelId: this.channelId,
      targetSiteId: targetSiteId,
      targetChannelId: targetChannelId,
      translateType: translateType,
      summary: summary
    });
  },

  updateGroups: function(res, message) {
    this.groupNames = res.groupNames;
    utils.success(message);
  },

  apiGet: function() {
    var $this = this;

    window.onresize = function() {
      $this.mainHeight = ($(window).height() - 70) + 'px';
    };
    window.onresize();

    $api.get($url, {
      params: {
        siteId: $this.siteId,
        channelId: $this.channelId,
        contentId: $this.contentId
      }
    })
    .then(function(response) {
      var res = response.data;

      $this.site = res.site;
      $this.siteUrl = res.siteUrl;
      $this.channel = res.channel;
      $this.groupNames = res.groupNames;
      $this.tagNames = res.tagNames;
      $this.checkedLevels = res.checkedLevels;
      
      $this.siteOptions = res.siteOptions;
      $this.channelOptions = res.channelOptions;

      $this.styles = res.styles;
      $this.form = _.assign({}, res.content);
      if ($this.form.checked) {
        $this.form.checkedLevel = $this.site.checkContentLevel;
      }
      if ($this.form.top || $this.form.recommend || $this.form.hot || $this.form.color) {
        $this.collapseSettings.push('attributes');
      }
      if ($this.form.groupNames && $this.form.groupNames.length > 0) {
        $this.collapseSettings.push('groupNames');
      } else {
        $this.form.groupNames = [];
      }
      if ($this.form.tagNames && $this.form.tagNames.length > 0) {
        $this.collapseSettings.push('tagNames');
      } else {
        $this.form.tagNames = [];
      }
      if ($this.form.linkUrl) {
        $this.collapseSettings.push('linkUrl');
      }

      for (var i = 0; i < $this.styles.length; i++) {
        var style = $this.styles[i];
        if (style.inputType !== 'Image' && style.inputType !== 'File' && style.inputType !== 'Video') continue;
        
        $this.form[utils.getCountName(style.attributeName)] = utils.toInt($this.form[utils.getCountName(style.attributeName)]);
      }

      setTimeout(function () {
        for (var i = 0; i < $this.styles.length; i++) {
          var style = $this.styles[i];
          if (style.inputType === 'TextEditor') {
            var editor = UE.getEditor(style.attributeName, {
              allowDivTransToP: false,
              maximumWords: 99999999
            });
            editor.styleIndex = i;
            editor.ready(function () {
              editor.addListener("contentChange", function () {
                var style = $this.styles[this.styleIndex];
                $this.form[_.lowerFirst(style.attributeName)] = this.getContent();
              });
            });
          }
        }
      }, 100);
    })
    .catch(function(error) {
      utils.error(error);
    })
    .then(function() {
      utils.loading($this, false);
    });
  },

  apiInsert: function() {
    var $this = this;

    utils.loading(this, true);
    $api.post($url, {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      content: this.form,
      translates: this.translates
    }).then(function(response) {
      var res = response.data;

      $this.closeAndRedirect();
    })
    .catch(function(error) {
      utils.error(error);
    })
    .then(function() {
      utils.loading($this, false);
    });
  },

  apiPreview: function() {
    var $this = this;

    utils.loading(this, true);
    $api.post($url + '/actions/preview', {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      content: this.form
    }).then(function(response) {
      var res = response.data;

      $this.isPreviewSaving = false;
      window.open(res.url);
    })
    .catch(function(error) {
      utils.error(error);
    })
    .then(function() {
      utils.loading($this, false);
    });
  },

  apiUpdate: function() {
    var $this = this;

    utils.loading(this, true);
    $api.put($url, {
      siteId: this.siteId,
      channelId: this.channelId,
      contentId: this.contentId,
      content: this.form,
      translates: this.translates
    }).then(function(response) {
      var res = response.data;

      $this.closeAndRedirect();
    })
    .catch(function(error) {
      utils.error(error);
    })
    .then(function() {
      utils.loading($this, false);
    });
  },

  closeAndRedirect: function(isEdit) {
    var tabVue = utils.getTabVue(this.tabName);
    if (tabVue) {
      if (isEdit) {
        tabVue.apiList(this.channelId, this.page, '内容保存成功！');
      } else {
        tabVue.apiList(this.channelId, this.page, '内容保存成功！', true);
      }
    }
    utils.removeTab();
    utils.openTab(this.tabName);
  },

  btnLayerClick: function(options) {
    var query = {
      siteId: this.siteId,
      channelId: this.channelId,
      editorAttributeName: 'Body'
    };

    if (options.contentId) {
      query.contentId = options.contentId;
    }
    if (options.attributeName) {
      query.attributeName = options.attributeName;
    }
    if (options.no) {
      query.no = options.no;
    }

    var args = {
      title: options.title,
      url: utils.getCommonUrl(options.name, query)
    };
    if (!options.full) {
      args.width = options.width ? options.width : 700;
      args.height = options.height ? options.height : 500;
    }

    utils.openLayer(args);
  },

  handleTranslationClose: function(summary) {
    this.translates = _.remove(this.translates, function(n) {
      return summary !== n.summary;
    });
  },

  btnSaveClick: function() {
    var $this = this;

    if (UE) {
      $.each(UE.instants, function (index, editor) {
        editor.sync();
      });
    }
    
    this.$refs.form.validate(function(valid) {
      if (valid) {
        if ($this.contentId === 0) {
          $this.apiInsert();
        } else {
          $this.apiUpdate();
        }
      } else {
        utils.error('保存失败，请检查表单值是否正确');
      }
    });
  },

  btnPreviewClick: function() {
    var $this = this;

    if (this.isPreviewSaving) return;

    if (UE) {
      $.each(UE.instants, function (index, editor) {
        editor.sync();
      });
    }
    
    this.$refs.form.validate(function(valid) {
      if (valid) {
        $this.apiPreview();
      } else {
        utils.error('预览失败，请检查表单值是否正确');
      }
    });
  },

  btnCloseClick: function() {
    utils.removeTab();
  },

  btnGroupAddClick: function() {
    utils.openLayer({
      title: '新增内容组',
      url: utils.getCommonUrl('groupContentLayerAdd', {siteId: this.siteId}),
      width: 500,
      height: 300
    });
  },

  btnTranslateAddClick: function() {
    utils.openLayer({
      title: "选择转移栏目",
      url: utils.getCmsUrl('editorLayerTranslate', {
        siteId: this.siteId,
        channelId: this.channelId
      }),
      width: 620,
      height: 400
    });
  },

  btnExtendAddClick: function(style) {
    var no = this.form[utils.getCountName(style.attributeName)] + 1;
    this.form[utils.getCountName(style.attributeName)] = no;
    this.form[utils.getExtendName(style.attributeName, no)] = '';
    this.form = _.assign({}, this.form);
  },

  btnExtendRemoveClick: function(style) {
    var no = this.form[utils.getCountName(style.attributeName)];
    this.form[utils.getCountName(style.attributeName)] = no - 1;
    this.form[utils.getExtendName(style.attributeName, no)] = '';
    this.form = _.assign({}, this.form);
  },

  btnExtendPreviewClick: function(attributeName, no) {
    var count = this.form[utils.getCountName(attributeName)];
    var data = [];
    for (var i = 0; i <= count; i++) {
      var imageUrl = this.form[utils.getExtendName(attributeName, i)];
      imageUrl = utils.getUrl(this.siteUrl, imageUrl);
      data.push({
        "src": imageUrl
      });
    }
    layer.photos({
      photos: {
        "start": no,
        "data": data
      }
      ,anim: 5
    });
  }
};

var $vue = new Vue({
  el: "#main",
  data: data,
  methods: methods,
  created: function() {
    this.apiGet();
  }
});
