import { createVNode } from 'vue'
import * as $Icon from '@ant-design/icons-vue'

export const Icon = (icon) => {
  if (!icon.icon) {
    return null
  }
  return createVNode($Icon[icon.icon])
}
