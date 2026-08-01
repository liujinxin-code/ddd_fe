<template>
  <div class="logo-container">
    <div class="logo">
      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor">
        <path
          fill-rule="evenodd"
          d="M7 2a1 1 0 012 0v1h2V2a1 1 0 112 0v1h2a2 2 0 012 2v2h1a1 1 0 110 2h-1v2h1a1 1 0 110 2h-1v2a2 2 0 01-2 2h-2v1a1 1 0 11-2 0v-1H9v1a1 1 0 11-2 0v-1H5a2 2 0 01-2-2v-2H2a1 1 0 110-2h1V9H2a1 1 0 010-2h1V5a2 2 0 012-2h2V2zM5 5h10v10H5V5z"
          clip-rule="evenodd"
        ></path>
      </svg>
    </div>
    <div class="logo-text"><span>Social</span><span class="boost">Boost</span></div>
  </div>
</template>
<style scoped lang="scss">
.logo-container {
  display: flex;
  align-items: center;
  .logo {
    background: #586ee1;
    width: 2.5rem;
    height: 2.5rem;
    border-radius: 10px;
    align-items: center;
    justify-content: center;
    display: grid;
    color: #ffffff;
    margin-right: 0.2rem;
  }
  .logo-text {
    padding-left: 0.8rem;
    .boost {
      color: #2f9e83;
      background: none;
      -webkit-text-fill-color: currentColor;
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }
  }
}
</style>
