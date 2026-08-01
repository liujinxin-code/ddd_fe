<script setup>
import { ConfigProvider } from 'ant-design-vue'
import zhCN from 'ant-design-vue/es/locale/zh_CN'
import dayjs from 'dayjs' // 引入中文包
import 'dayjs/locale/zh-cn' // 引入中文包
dayjs.locale('zh-cn') // 设置语言[4](@ref)

const themeConfig = {
  token: {
    colorPrimary: '#586ee1',
    colorPrimaryHover: '#6c7fe8',
    colorBgBase: '#eef2f5',
    colorBgLayout: '#eef2f5',
    colorBgContainer: '#f8fafb',
    colorBgElevated: '#fbfcfd',
    colorText: '#26313d',
    colorTextSecondary: '#65727f',
    colorBorder: '#d5dde3',
    colorSplit: '#dce3e8',
    borderRadius: 6,
  },
}
</script>
<template>
  <ConfigProvider :locale="zhCN" :theme="themeConfig">
    <router-view></router-view>
  </ConfigProvider>
</template>

<style lang="scss">
body {
  margin: 0;
  min-width: 320px;
  min-height: 100vh;
  background: #eef2f5;
}
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  color: #253142;
  width: 100%;
  min-height: 100vh;
}

/* Application-wide light theme overrides legacy dark scoped styles. */
#app .login-page {
  background: #eef2f5;
  background-image: none;
  color: #26313d;
}

#app .login-page .tips1,
#app .login-page .brand-area h2,
#app .login-page .auth-form .ant-form-item-label > label,
#app .login-page .ant-tabs-nav-wrap {
  color: #253142;
}

#app .login-page .brand-area p {
  color: #68768a;
}

#app .login-page .tips1 .tip,
#app .login-page .captcha-box {
  color: #586ee1;
}

#app .login-page .captcha-box {
  border-color: #aeb9ee;
}

#app .login-page .submit-btn {
  background: #586ee1;
}

#app .login-page .login-card {
  background: #f8fafb;
  border-color: #d5dde3;
  box-shadow: 0 16px 40px rgba(38, 49, 61, 0.11);
}

#app .homeIndex-container,
#app .agent-page,
#app .order-page,
#app .consume-page {
  color: #253142;
}

#app .homeIndex-container .tips1,
#app .homeIndex-container .input-search,
#app .homeIndex-container .select-search {
  color: #253142;
}

#app .homeIndex-container .tips2,
#app .homeIndex-container .config-name,
#app .homeIndex-container .config-num,
#app .agent-page .page-header p,
#app .agent-page .tips,
#app .agent-page dt,
#app .agent-page .stats span,
#app .order-page header p,
#app .order-page dt,
#app .consume-page header p,
#app .consume-page dt {
  color: #68768a;
}

#app .homeIndex-container .search-container,
#app .homeIndex-container .select-search,
#app .homeIndex-container .config-item,
#app .homeIndex-container .loading-more,
#app .agent-page .section,
#app .agent-page .stats .ant-card,
#app .order-page .section,
#app .consume-page .section {
  background: #f8fafb;
  border-color: #d5dde3;
  box-shadow: 0 5px 16px rgba(38, 49, 61, 0.06);
}

#app .homeIndex-container .search-container,
#app .homeIndex-container .select-search,
#app .homeIndex-container .loading-more {
  border-color: #cbd5dc;
}

/* Ant Select owns the visible selector border; the wrapper must stay flat so
   the filter row does not render a doubled outline. */
#app .homeIndex-container .select-search {
  background: transparent;
  border: 0;
  box-shadow: none;
}

#app .homeIndex-container .product-name,
#app .agent-page .item,
#app .order-page .item,
#app .consume-page .item {
  background: #e9eef2;
  border-color: #d5dde3;
  color: #26313d;
}

#app .homeIndex-container .product-name {
  color: #3e4a56;
}

#app .homeIndex-container .config-amount,
#app .homeIndex-container .estimated {
  color: #586ee1;
}

.ant-modal-root .ant-form-item-label > label {
  color: #253142 !important;
}

#app .homeIndex-container { padding: 2rem 0 3rem; }
#app .homeIndex-container .header-container { padding: 1.5rem 0 1.75rem; }
#app .homeIndex-container .search-container { border-radius: 10px; box-shadow: none; }
#app .homeIndex-container .config-container { grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 1.25rem; }
#app .homeIndex-container .config-item { border-radius: 10px; box-shadow: none; }
#app .homeIndex-container .config-item:hover { border-color: #aeb9ee; box-shadow: 0 8px 20px rgba(88, 110, 225, 0.1); }
#app .homeIndex-container .button-search { background: #586ee1; color: #ffffff; border-radius: 7px; }
#app .homeIndex-container .button-search:hover { background: #6c7fe8; }
#app .agent-page .stats strong { color: #586ee1; }
#app .order-page a { color: #586ee1; }
</style>
