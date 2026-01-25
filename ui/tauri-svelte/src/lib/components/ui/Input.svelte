<script lang="ts">
  import type { HTMLInputAttributes } from 'svelte/elements';

  interface Props extends HTMLInputAttributes {
    label?: string;
    error?: string;
    value?: string;
  }

  let { 
    label,
    error,
    value = $bindable(''),
    class: className = '',
    id,
    ...restProps 
  }: Props = $props();

  const inputId = id ?? `input-${Math.random().toString(36).slice(2)}`;
</script>

<div class="input-wrapper {className}">
  {#if label}
    <label for={inputId} class="input-label">{label}</label>
  {/if}
  <input 
    id={inputId}
    class="input" 
    class:has-error={!!error}
    bind:value
    {...restProps}
  />
  {#if error}
    <span class="input-error">{error}</span>
  {/if}
</div>

<style>
  .input-wrapper {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .input-label {
    font-size: 14px;
    font-weight: 500;
    color: #e0e0e0;
  }

  .input {
    width: 100%;
    padding: 12px 14px;
    background-color: var(--bg-secondary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    color: var(--text-primary);
    font-size: 14px;
    font-family: inherit;
    box-sizing: border-box;
    transition: border-color 0.15s;
    box-shadow: none;
  }

  .input:focus {
    outline: none;
    border-color: var(--accent-color);
  }

  .input::placeholder {
    color: var(--text-muted);
  }

  .input.has-error {
    border-color: #ff6b6b;
  }

  .input.has-error:focus {
    border-color: #ff6b6b;
  }

  .input-error {
    font-size: 12px;
    color: #ff6b6b;
  }
</style>
