<script>
    let email = $state("");
    let password = $state("");

    function print(){
        console.log(email);
        console.log(password);
    }
</script>

<h3>Регистрация</h3>

<div style="border: 1px solid #ccc; padding: 20px;">
<label>
<input bind:value={email} type="text" placeholder="Email"/>
</label>

<label>
<input bind:value={password} type="text" placeholder="Password"/>
</label>
</div>

<button onclick={print}>
Вывести в консоль
</button>