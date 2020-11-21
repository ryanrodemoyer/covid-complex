<template>
  <div class="container-fluid">
    <div class="row">
      <div class="col">
        <h1 class="green">{{message}}</h1>

        <div class="card">
          <div class="card-header">header</div>

          <div class="card-body">
            <Child></Child>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.green {
  color: green;
}
</style>

<script>
// import Child from "./Child.vue";

export default {
  components: { },
  data: () => ({
    message: "hey rodey from the page",
  }),
};
</script>