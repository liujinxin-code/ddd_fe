<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { Image, Modal, Spin, message } from 'ant-design-vue'
import { resolveAssetUrl, serviceImageApi } from '../api'
import { Icon } from '../assets/js/iconUtils.js'

const wechatVisible = ref(false)
const wechatLoading = ref(false)
const wechatImage = ref({ imageUrl: '', agentUserid: 0 })

const openWechat = async () => {
  wechatLoading.value = true
  wechatVisible.value = true
  try {
    const result = await serviceImageApi.myWechat()
    const data = result.data || { imageUrl: '', agentUserid: 0 }
    wechatImage.value = {
      ...data,
      imageUrl: resolveAssetUrl(data.imageUrl),
    }
  } catch (error) {
    message.error(error.message)
  } finally {
    wechatLoading.value = false
  }
}

// ═══ 可拖拽定位 ═══
const STORAGE_KEY = 'wechat_fab_pos'
const translate = ref({ x: 0, y: 0 })
const isDragging = ref(false)
const dragStart = ref({ x: 0, y: 0, mx: 0, my: 0 })
const buttonRef = ref(null)
const modalWidth = ref(420) // PC 端弹窗宽一点，二维码尺寸固定方便扫码

const MARGIN = 8 // 贴边安全边距

const getInitialOffset = () => {
  // 从 CSS 的 right/bottom 百分比拿到初始像素偏移，
  // 拖拽范围据此计算，才能保证按钮能真正拖到四边。
  const el = buttonRef.value
  if (!el) {
    return {
      right: window.innerWidth <= 768 ? window.innerWidth * 0.05 : window.innerWidth * 0.10,
      bottom: window.innerHeight <= 768 ? window.innerHeight * 0.10 : window.innerHeight * 0.15,
    }
  }
  const style = window.getComputedStyle(el)
  return {
    right: parseFloat(style.right) || 0,
    bottom: parseFloat(style.bottom) || 0,
  }
}

const getButtonSize = () => {
  const rect = buttonRef.value?.getBoundingClientRect()
  return {
    width: rect?.width ?? 120,
    height: rect?.height ?? 60,
  }
}

const restorePosition = () => {
  try {
    const saved = localStorage.getItem(STORAGE_KEY)
    if (saved) {
      const pos = JSON.parse(saved)
      if (typeof pos.x === 'number' && typeof pos.y === 'number') {
        translate.value = clampPosition(pos.x, pos.y)
      }
    }
  } catch {
    // ignore
  }
}

const clampPosition = (x, y) => {
  const { width, height } = getButtonSize()
  const { right, bottom } = getInitialOffset()
  // 按钮 CSS 为 position: fixed; right: X; bottom: Y;
  // translate(tx, ty) 在初始位置基础上偏移。
  // 要允许拖到屏幕四边，tx/ty 范围必须同时包含正负两个方向：
  //   最左/上  ->  元素贴到屏幕边缘
  //   最右/下  ->  元素回到初始 right/bottom 贴边处
  const minX = right + width + MARGIN - window.innerWidth
  const maxX = right - MARGIN
  const minY = bottom + height + MARGIN - window.innerHeight
  const maxY = bottom - MARGIN
  return {
    x: Math.max(minX, Math.min(x, maxX)),
    y: Math.max(minY, Math.min(y, maxY)),
  }
}

const savePosition = () => {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(translate.value))
  } catch {
    // ignore
  }
}

const onMouseDown = (e) => {
  isDragging.value = false
  dragStart.value = { x: translate.value.x, y: translate.value.y, mx: e.clientX, my: e.clientY }
  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

const DRAG_THRESHOLD = 10 // 移动端手指轻微抖动容差，超过才算拖拽

const onMouseMove = (e) => {
  const dx = e.clientX - dragStart.value.mx
  const dy = e.clientY - dragStart.value.my
  if (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD) {
    isDragging.value = true
  }
  translate.value = clampPosition(dragStart.value.x + dx, dragStart.value.y + dy)
}

const onMouseUp = () => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
  savePosition()
  // 拖拽后浏览器仍会合成 click，用标志拦截，避免弹窗误开
  if (isDragging.value) {
    suppressClick.value = true
  }
}

const onTouchStart = (e) => {
  const touch = e.touches[0]
  isDragging.value = false
  dragStart.value = { x: translate.value.x, y: translate.value.y, mx: touch.clientX, my: touch.clientY }
  document.addEventListener('touchmove', onTouchMove, { passive: false })
  document.addEventListener('touchend', onTouchEnd)
}

const onTouchMove = (e) => {
  const touch = e.touches[0]
  const dx = touch.clientX - dragStart.value.mx
  const dy = touch.clientY - dragStart.value.my
  // 仅在确实超过阈值（真正拖拽）时才阻止默认行为，
  // 否则保留浏览器合成的 click 事件，确保轻触能正常打开弹窗
  if (Math.abs(dx) > DRAG_THRESHOLD || Math.abs(dy) > DRAG_THRESHOLD) {
    isDragging.value = true
    e.preventDefault()
  }
  translate.value = clampPosition(dragStart.value.x + dx, dragStart.value.y + dy)
}

const onTouchEnd = () => {
  document.removeEventListener('touchmove', onTouchMove)
  document.removeEventListener('touchend', onTouchEnd)
  savePosition()
  // 不再在此处打开弹窗：未拖拽时由浏览器合成的 click 事件触发 onClick
}

// 用 click 统一处理“打开弹窗”，拖拽时拦截
const suppressClick = ref(false)
const onClick = () => {
  if (suppressClick.value) {
    suppressClick.value = false
    return
  }
  openWechat()
}

const onResize = () => {
  translate.value = clampPosition(translate.value.x, translate.value.y)
  modalWidth.value = window.innerWidth <= 768 ? 360 : 420
  savePosition()
}

onMounted(() => {
  restorePosition()
  window.addEventListener('resize', onResize)
})

onUnmounted(() => {
  document.removeEventListener('mousemove', onMouseMove)
  document.removeEventListener('mouseup', onMouseUp)
  document.removeEventListener('touchmove', onTouchMove)
  document.removeEventListener('touchend', onTouchEnd)
  window.removeEventListener('resize', onResize)
})
</script>

<template>
  <div
    ref="buttonRef"
    class="draggable-wechat"
    :class="{ 'is-dragging': isDragging }"
    :style="{ transform: `translate3d(${translate.x}px, ${translate.y}px, 0)` }"
    @mousedown="onMouseDown"
    @touchstart="onTouchStart"
    @click="onClick"
  >
    <Icon icon="WechatOutlined" />
    <span>添加微信</span>
  </div>

  <Modal
    v-model:open="wechatVisible"
    title="添加微信"
    :footer="null"
    centered
    :width="modalWidth"
  >
    <Spin :spinning="wechatLoading">
      <div class="wechat-modal-body">
        <p v-if="!wechatImage.imageUrl" class="wechat-empty">暂无客服微信图片</p>
        <Image
          v-else
          :src="wechatImage.imageUrl"
          alt="客服微信"
          class="wechat-qrcode"
          preview
        />
        <p class="wechat-tip">截图保存二维码，使用微信扫一扫添加</p>
      </div>
    </Spin>
  </Modal>
</template>

<style scoped lang="scss">
.draggable-wechat {
  position: fixed;
  right: 10%;
  bottom: 15%;
  z-index: 1000;
  display: flex;
  align-items: center;
  gap: 0.35rem;
  padding: 0 1rem;
  min-width: 56px;
  height: 56px;
  border-radius: 999px;
  border: 0;
  background: linear-gradient(135deg, #07c160 0%, #05a350 100%);
  color: #fff;
  font-size: 0.85rem;
  font-weight: 600;
  box-shadow: 0 4px 14px rgba(7, 193, 96, 0.35);
  cursor: grab;
  user-select: none;
  touch-action: none;
  transition: transform 0.1s, box-shadow 0.2s;
  :deep(.anticon) { font-size: 1.15rem; }
  &:hover { box-shadow: 0 6px 20px rgba(7, 193, 96, 0.45); }
  &:active { cursor: grabbing; }
}
.draggable-wechat.is-dragging {
  transition: none;
  cursor: grabbing;
}

.wechat-modal-body {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem 0 0.25rem;
}
.wechat-qrcode {
  width: 100%;
  max-height: 520px;
  border-radius: 8px;
  border: 1px solid #f1f5f9;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  :deep(.ant-image-img) {
    width: 100%;
    height: auto;
    object-fit: contain;
  }
}
.wechat-empty { color: #9ca3af; font-size: 0.9rem; margin: 2rem 0; }
.wechat-tip { color: #7a8794; font-size: 0.8rem; margin: 0; text-align: center; }

@media (max-width: 768px) {
  .draggable-wechat {
    right: 5%;
    bottom: 10%;
    width: auto;
    min-width: 52px;
    height: 52px;
    padding: 0 0.75rem;
    justify-content: center;
    font-size: 0.75rem;
    white-space: nowrap;
  }
  .wechat-qrcode {
    width: 100%;
    max-height: 260px;
    :deep(.ant-image-img) {
      height: auto;
    }
  }
}
</style>
