/**
 * 金额/价格统一格式化
 * - 对齐后端 decimal(11,6) 精度，最多保留 6 位小数
 * - 去掉末尾无意义的 0
 * - 整数不补 .00
 * @param {number|string|null|undefined} value
 * @returns {string}
 */
export const formatMoney = (value) => {
  return Number(value || 0).toLocaleString('zh-CN', {
    minimumFractionDigits: 0,
    maximumFractionDigits: 6,
    useGrouping: false,
  })
}
