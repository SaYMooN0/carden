<script>
    let email = $state("");
    let password = $state("");
    let confirmPassword =$state("")
    let isLoginMode=$state(false);
    let {onLogin} = $props();

    function toggleMode(){
        isLoginMode = !isLoginMode;
    }
</script>

<h2>{isLoginMode ? "Вход" : "Регистрация"}</h2>

<div style="border: 1px solid #ccc; padding: 20px;">
<label>
<input bind:value={email} type="text" placeholder="Почта"/>
</label>

<label>
<input bind:value={password} type="text" placeholder="Пароль"/>
</label>
{#if !isLoginMode}
<label>
<input bind:value={confirmPassword} type="text" placeholder="Подтвердите пароль"/>
</label>
{/if}
</div>

<button onclick={toggleMode}>
{isLoginMode ? "У меня уже есть аккаунт" : "У меня еще нет аккаунта"}
</button>

<button onclick={onLogin}>
{isLoginMode ? "Войти" : "Зарегистрироваться"}
</button>