<script lang="ts">
</script>

<div class="carden-container">
	<header class="navbar">
		<h1>carden</h1>
		<div class="actions">
			<button class="nav-btn">Dashboard</button>
			<button class="nav-btn outline">Profile</button>
		</div>
	</header>

	<main class="content">
		<h2>Welcome back!</h2>
		<p>This is your primary dashboard view.</p>
	</main>
</div>

<style>
	.carden-container {
		width: 100%;
		min-height: 100vh;
		display: flex;
		flex-direction: column;
	}

	.navbar {
		display: flex;
		justify-content: space-between;
		align-items: center;
		padding: 1rem 2rem;
		background-color: var(--color-sage);
		color: var(--text);
		box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
	}

	.navbar h1 {
		font-size: 1.5rem;
		font-weight: 700;
		color: var(--color-terracotta);
		margin: 0;
	}

	.actions {
		display: flex;
		gap: 1rem;
	}

	.nav-btn {
		padding: 0.5rem 1rem;
		border: none;
		border-radius: 0.375rem;
		background-color: var(--color-terracotta);
		color: var(--color-cream);
		font-weight: 500;
		cursor: pointer;
		transition: background-color 0.2s;
	}

	.nav-btn:hover {
		background-color: var(--color-terracotta-hover);
	}

	.nav-btn.outline {
		background-color: transparent;
		border: 2px solid var(--color-terracotta);
		color: var(--color-terracotta);
	}

	.nav-btn.outline:hover {
		background-color: var(--color-terracotta);
		color: var(--color-cream);
	}

	.content {
		flex: 1;
		padding: 2rem;
		max-width: 1200px;
		margin: 0 auto;
		width: 100%;
	}

	h2 {
		color: var(--text);
		margin-bottom: 1rem;
		font-size: 2rem;
	}

	p {
		color: var(--color-text-light, #64748b);
		font-size: 1.125rem;
	}
</style>
