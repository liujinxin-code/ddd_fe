<script setup>
import { computed, onMounted, ref } from 'vue'
import { Dropdown, Menu, MenuItem, Modal, Spin, message } from 'ant-design-vue'
import { useRouter } from 'vue-router'
import IconLogo from '../components/icons/IconLogo.vue'
import { Icon } from '../assets/js/iconUtils.js'
import ChangePasswordModal from '../components/agent/ChangePasswordModal.vue'
import { authApi, noticeApi } from '../api'
import { useAuth } from '../stores/auth'

const auth = useAuth()
const router = useRouter()
const user = computed(() => auth.user.value || {})
const changePasswordModalVisible = ref(false)
const announcementVisible = ref(false)
const announcementLoading = ref(false)
const visibleAnnouncementCount = ref(2)

// 公告数据来自后端 Notice 接口（置顶 type=1 优先，普通 type=2 在时间线）。
// 字段映射：noticeId/noticeContent/noticeType/createTime → { id, title, content, date, type, pinned }
const announcements = ref([])
const popupVisible = ref(false)
const popupContent = ref('')

const loadNotices = async () => {
  try {
    const result = await noticeApi.homepage({ page: 1, pageSize: 6 })
    announcements.value = (result.data || []).map((n) => ({
      id: n.noticeId,
      title: n.noticeType === 1 ? '置顶公告' : '平台公告',
      content: n.noticeContent,
      date: n.createTime ? new Date(n.createTime).toISOString().slice(0, 10) : '',
      type: n.noticeType === 1 ? 'release' : 'update',
      pinned: n.noticeType === 1,
    }))
  } catch {
    // 占位：公告加载失败时不阻塞页面，保持空列表
  }
}

const loadPopup = async () => {
  try {
    const result = await noticeApi.popup()
    if (result.data) {
      popupContent.value = result.data.noticeContent
      popupVisible.value = true
    }
  } catch {
    // 占位：弹窗公告加载失败忽略
  }
}

const pinnedAnnouncement = computed(() => announcements.value.find((item) => item.pinned))
const timelineAnnouncements = computed(() => announcements.value.filter((item) => !item.pinned))
const visibleTimelineAnnouncements = computed(() => timelineAnnouncements.value.slice(0, visibleAnnouncementCount.value))
const hasMoreAnnouncements = computed(() => visibleTimelineAnnouncements.value.length < timelineAnnouncements.value.length)

const openAnnouncements = () => {
  visibleAnnouncementCount.value = 2
  announcementVisible.value = true
}

const loadMoreAnnouncements = () => {
  if (announcementLoading.value || !hasMoreAnnouncements.value) return
  announcementLoading.value = true
  window.setTimeout(() => {
    visibleAnnouncementCount.value = Math.min(
      visibleAnnouncementCount.value + 2,
      timelineAnnouncements.value.length,
    )
    announcementLoading.value = false
  }, 180)
}

const handlePasswordSubmit = async (payload) => {
  try {
    await authApi.changePassword({ oldPassword: payload.oldPassword, newPassword: payload.newPassword })
    message.success('密码修改成功')
  } catch (error) {
    message.error(error.message)
  }
}

const handleLogout = async () => {
  await auth.logout()
  await router.replace('/login')
}

onMounted(async () => {
  try {
    await auth.loadUser()
  } catch (error) {
    if (error.code !== 401) message.error(error.message)
  }
  await loadNotices()
  await loadPopup()
})
</script>

<template>
  <div class="app-shell">
    <main class="page-main">
      <header class="topbar">
        <IconLogo />
        <nav class="desktop-nav" aria-label="主导航">
          <router-link to="/app/homeIndex" active-class="active">首页</router-link>
          <router-link v-if="user.isAgent" to="/app/agent" active-class="active">代理管理</router-link>
          <router-link to="/app/order" active-class="active">订单列表</router-link>
          <router-link to="/app/consumption" active-class="active">消费列表</router-link>
        </nav>
        <div class="user-info">
          <div class="balance-info">
            <span>{{ user.isAgent ? '代理用户' : '普通用户' }}</span>
            <strong>余额 ¥{{ Number(user.userAmount || 0).toFixed(2) }}</strong>
          </div>
          <Dropdown>
            <a class="user-trigger" @click.prevent>
              <span class="user-avatar">{{ (user.username || 'U').slice(0, 1).toUpperCase() }}</span>
              <span>{{ user.username || '用户' }}</span>
              <Icon icon="DownOutlined" />
            </a>
            <template #overlay>
              <Menu>
                <MenuItem><router-link to="/app/homeIndex"><Icon icon="HomeOutlined" /> 首页</router-link></MenuItem>
                <MenuItem v-if="user.isAgent"><router-link to="/app/agent"><Icon icon="UsergroupAddOutlined" /> 代理管理</router-link></MenuItem>
                <MenuItem><router-link to="/app/order"><Icon icon="OrderedListOutlined" /> 订单列表</router-link></MenuItem>
                <MenuItem><router-link to="/app/consumption"><Icon icon="MenuUnfoldOutlined" /> 消费列表</router-link></MenuItem>
                <MenuItem @click="changePasswordModalVisible = true"><span><Icon icon="LockOutlined" /> 修改密码</span></MenuItem>
                <MenuItem danger><a href="javascript:;" @click.prevent="handleLogout"><Icon icon="ClearOutlined" /> 退出登录</a></MenuItem>
              </Menu>
            </template>
          </Dropdown>
        </div>
      </header>

      <section
        class="announcement-panel"
        aria-labelledby="announcement-title"
        role="button"
        tabindex="0"
        :aria-expanded="announcementVisible"
        @click="openAnnouncements"
        @keydown.enter.prevent="openAnnouncements"
        @keydown.space.prevent="openAnnouncements"
      >
        <div class="announcement-heading">
          <div class="announcement-heading-main">
            <span class="announcement-heading-icon"><Icon icon="BellFilled" /></span>
            <div>
              <h2 id="announcement-title">平台公告</h2>
              <p>重要通知、产品更新与服务说明</p>
            </div>
          </div>
          <span class="announcement-live"><i></i>实时更新</span>
        </div>

        <div v-if="pinnedAnnouncement" class="announcement-pinned announcement-pinned-preview">
          <div class="announcement-pinned-mark"><Icon icon="PushpinFilled" /></div>
          <div class="announcement-pinned-body">
            <div class="announcement-meta"><span>置顶公告</span><time>{{ pinnedAnnouncement.date }}</time></div>
            <strong>{{ pinnedAnnouncement.title }}</strong>
            <p>{{ pinnedAnnouncement.content }}</p>
          </div>
          <span class="announcement-view-all">查看全部 <Icon icon="RightOutlined" /></span>
        </div>
        <div v-else class="announcement-empty-preview">当前暂无置顶公告，点击查看全部公告</div>
      </section>

      <Modal
        v-model:open="announcementVisible"
        class="announcement-modal"
        title="平台公告"
        :footer="null"
        width="680px"
        centered
      >
        <div class="announcement-dialog-content">
          <div v-if="pinnedAnnouncement" class="announcement-pinned">
            <div class="announcement-pinned-mark"><Icon icon="PushpinFilled" /></div>
            <div class="announcement-pinned-body">
              <div class="announcement-meta"><span>置顶公告</span><time>{{ pinnedAnnouncement.date }}</time></div>
              <strong>{{ pinnedAnnouncement.title }}</strong>
              <p>{{ pinnedAnnouncement.content }}</p>
            </div>
          </div>

          <div v-if="visibleTimelineAnnouncements.length" class="announcement-timeline">
            <div v-for="item in visibleTimelineAnnouncements" :key="item.id" class="announcement-timeline-item">
            <span class="announcement-timeline-marker" :class="`is-${item.type}`"></span>
            <div class="announcement-timeline-content">
              <div class="announcement-meta"><span>{{ item.type === 'release' ? '产品更新' : '服务动态' }}</span><time>{{ item.date }}</time></div>
              <strong>{{ item.title }}</strong>
              <p>{{ item.content }}</p>
            </div>
            </div>
          </div>

          <div class="announcement-load-state">
            <Spin v-if="announcementLoading" size="small" />
            <button v-else-if="hasMoreAnnouncements" type="button" class="announcement-load-more" @click="loadMoreAnnouncements">
              加载更早公告
              <Icon icon="DownOutlined" />
            </button>
            <span v-else>已加载全部公告</span>
          </div>
        </div>
      </Modal>

      <section class="content-container"><RouterView /></section>
    </main>

    <ChangePasswordModal v-model:open="changePasswordModalVisible" @submit="handlePasswordSubmit" />

    <Modal
      v-model:open="popupVisible"
      class="announcement-popup"
      title="公告"
      :footer="null"
      width="560px"
      centered
    >
      <p class="announcement-popup-content">{{ popupContent }}</p>
    </Modal>

    <footer class="site-footer">
      <IconLogo />
      <span>2026 SocialBoost. All rights reserved. 专业社交媒体增长服务平台</span>
      <a href="mailto:support@example.com">联系我们</a>
    </footer>
  </div>
</template>

<style scoped lang="scss">
.app-shell { min-height: 100vh; display: flex; flex-direction: column; background: #eef2f5; color: #26313d; }
.page-main { flex: 1 0 auto; display: flex; flex-direction: column; min-height: 0; }
.topbar { min-height: 64px; padding: 0 1.5rem; background: #f8fafb; border-bottom: 1px solid #d5dde3; display: flex; align-items: center; justify-content: space-between; gap: 1rem; box-sizing: border-box; }
.desktop-nav { display: flex; align-items: center; gap: .35rem; }
.desktop-nav a { min-width: 76px; height: 38px; padding: 0 .65rem; display: flex; align-items: center; justify-content: center; color: #566371; text-decoration: none; border-radius: 6px; box-sizing: border-box; }
.desktop-nav a:hover { background: #e9eef2; color: #202a34; }
.desktop-nav .active { background: #edf0ff; color: #586ee1; font-weight: 600; }
.user-info,.user-trigger { display: flex; align-items: center; gap: .65rem; }
.balance-info { display: grid; text-align: right; font-size: .78rem; color: #65727f; }
.balance-info strong { color: #586ee1; font-weight: 600; }
.user-trigger { color: #3e4a56; text-decoration: none; }
.user-avatar { width: 38px; height: 38px; border-radius: 50%; display: grid; place-items: center; color: #ffffff; font-weight: 700; background: #586ee1; }
.announcement-panel { margin: 12px 1.5rem 0; padding: 1rem 1.1rem 1.1rem; background: #f8fafb; border: 1px solid #d5dde3; border-radius: 12px; box-sizing: border-box; cursor: pointer; transition: border-color .18s ease, box-shadow .18s ease; }
.announcement-panel:hover, .announcement-panel:focus-visible { border-color: #aeb9ee; outline: none; box-shadow: 0 6px 18px rgba(88,110,225,.08); }
.announcement-heading { display: flex; align-items: center; justify-content: space-between; gap: 1rem; padding-bottom: .8rem; border-bottom: 1px solid #e5eaee; }
.announcement-heading-main { display: flex; align-items: center; gap: .7rem; min-width: 0; }
.announcement-heading-icon { width: 30px; height: 30px; display: grid; place-items: center; flex: 0 0 auto; border-radius: 8px; background: #edf0ff; color: #586ee1; }
.announcement-heading h2 { margin: 0; color: #26313d; font-size: .98rem; line-height: 1.3; }
.announcement-heading p { margin: .15rem 0 0; color: #7a8794; font-size: .76rem; }
.announcement-live { display: inline-flex; align-items: center; gap: .35rem; flex: 0 0 auto; padding: .3rem .6rem; color: #3f805f; background: #e2f3e9; border-radius: 999px; font-size: .74rem; }
.announcement-live i { width: 6px; height: 6px; border-radius: 50%; background: #4d9b70; box-shadow: 0 0 0 3px rgba(77,155,112,.14); }
.announcement-pinned { display: flex; gap: .75rem; margin-top: .85rem; padding: .8rem .85rem; background: #edf0ff; border: 1px solid #dce3fb; border-radius: 9px; }
.announcement-pinned-preview { align-items: center; }
.announcement-pinned-preview .announcement-pinned-body { flex: 1; }
.announcement-pinned-preview p { display: -webkit-box; overflow: hidden; -webkit-box-orient: vertical; -webkit-line-clamp: 2; }
.announcement-pinned-mark { width: 28px; height: 28px; display: grid; place-items: center; flex: 0 0 auto; border-radius: 7px; background: #586ee1; color: #fff; }
.announcement-view-all { display: inline-flex; align-items: center; gap: .25rem; flex: 0 0 auto; color: #586ee1; font-size: .74rem; font-weight: 600; white-space: nowrap; }
.announcement-empty-preview { margin-top: .85rem; color: #71808f; font-size: .8rem; }
.announcement-pinned-body, .announcement-timeline-content { min-width: 0; }
.announcement-meta { display: flex; align-items: center; gap: .6rem; margin-bottom: .25rem; color: #71808f; font-size: .72rem; }
.announcement-meta span { color: #586ee1; font-weight: 600; }
.announcement-meta time { color: #8995a0; }
.announcement-pinned strong, .announcement-timeline-content strong { display: block; color: #26313d; font-size: .88rem; line-height: 1.4; }
.announcement-pinned p, .announcement-timeline-content p { margin: .25rem 0 0; color: #647281; font-size: .8rem; line-height: 1.55; }
.announcement-timeline { position: relative; display: grid; gap: .9rem; margin: .95rem 0 0 1.1rem; padding-left: 1.2rem; }
.announcement-timeline::before { position: absolute; top: .45rem; bottom: .45rem; left: .18rem; width: 1px; background: #d5dde3; content: ''; }
.announcement-timeline-item { position: relative; display: flex; }
.announcement-timeline-marker { position: absolute; top: .28rem; left: -1.2rem; width: 9px; height: 9px; border: 2px solid #f8fafb; border-radius: 50%; background: #8b98a5; box-shadow: 0 0 0 1px #c9d2da; }
.announcement-timeline-marker.is-release { background: #586ee1; box-shadow: 0 0 0 1px #aeb9ee; }
.announcement-timeline-marker.is-update { background: #4d9b70; box-shadow: 0 0 0 1px #a7d2b8; }
.announcement-dialog-content { max-height: min(66vh, 620px); overflow-y: auto; padding: .1rem .35rem .25rem .1rem; }
.announcement-dialog-content .announcement-pinned { margin-top: .1rem; }
.announcement-dialog-content .announcement-timeline { margin-top: 1.1rem; }
.announcement-load-state { display: flex; min-height: 34px; align-items: center; justify-content: center; color: #8995a0; font-size: .74rem; }
.announcement-load-more { display: inline-flex; align-items: center; gap: .3rem; padding: .35rem .65rem; border: 1px solid #d5dde3; border-radius: 6px; background: #f8fafb; color: #586ee1; font: inherit; font-size: .74rem; cursor: pointer; }
.announcement-load-more:hover { border-color: #aeb9ee; background: #edf0ff; }
.content-container { flex: 1; width: 100%; max-width: 1440px; margin: 0 auto; padding: 0 1.5rem; box-sizing: border-box; }
.site-footer { flex: 0 0 auto; min-height: 76px; padding: 1rem 1.5rem; background: #e6ebef; border-top: 1px solid #d1d9df; display: flex; align-items: center; justify-content: space-between; gap: 1rem; box-sizing: border-box; color: #65727f; }
.site-footer a { color: #465461; }
@media (max-width: 768px) {
  .topbar { padding: .75rem 1rem; align-items: flex-start; }
  .desktop-nav { display: none; }
  .user-info { margin-left: auto; }
  .balance-info { display: none; }
  .announcement-panel { margin: 10px 1rem 0; padding: .85rem; }
  .announcement-heading { align-items: flex-start; }
  .announcement-heading p { display: none; }
  .announcement-live { margin-top: .1rem; }
  .announcement-view-all { font-size: 0; }
  .announcement-view-all :deep(.anticon) { font-size: .78rem; }
  .announcement-timeline { margin-left: .7rem; padding-left: 1rem; }
  .announcement-timeline-marker { left: -1rem; }
  .content-container { padding: 0 1rem; }
  .site-footer { min-height: auto; flex-direction: column; text-align: center; }
}
</style>
