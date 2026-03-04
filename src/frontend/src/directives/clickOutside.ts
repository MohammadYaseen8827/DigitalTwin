import type { Directive } from 'vue'

const clickOutside: Directive = {
  mounted(el, binding) {
    el.__clickOutsideHandler__ = (event: MouseEvent) => {
      if (!el.contains(event.target as Node)) {
        binding.value(event)
      }
    }
    document.addEventListener('mousedown', el.__clickOutsideHandler__)
  },
  unmounted(el) {
    document.removeEventListener('mousedown', el.__clickOutsideHandler__)
    delete el.__clickOutsideHandler__
  }
}

export default clickOutside
