<script lang="ts">
  import type { Snippet } from 'svelte';
  import type { HTMLButtonAttributes } from 'svelte/elements';

  interface Props extends HTMLButtonAttributes {
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
    size?: 'sm' | 'md' | 'lg';
    children: Snippet;
  }

  let { 
    variant = 'primary', 
    size = 'md',
    children,
    class: className = '',
    ...restProps 
  }: Props = $props();
</script>

<button 
  class="btn btn-{variant} btn-{size} {className}"
  {...restProps}
>
  {@render children()}
</button>

<style>
  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border: none;
    border-radius: 20px;
    font-weight: 500;
    cursor: pointer;
    transition: background-color 0.15s;
    font-family: inherit;
    box-shadow: none;
  }

  .btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  /* Sizes */
  .btn-sm {
    padding: 6px 16px;
    font-size: 13px;
    height: 32px;
  }

  .btn-md {
    padding: 10px 24px;
    font-size: 14px;
    height: 36px;
  }

  .btn-lg {
    padding: 12px 28px;
    font-size: 16px;
    height: 44px;
  }

  /* Variants - Blazor dark theme style */
  .btn-primary {
    background-color: var(--accent-color);
    color: #ffffff;
  }

  .btn-primary:hover:not(:disabled) {
    background-color: var(--accent-color-hover);
  }

  .btn-secondary {
    background-color: var(--bg-hover);
    color: #e0e0e0;
  }

  .btn-secondary:hover:not(:disabled) {
    background-color: #4a4a4a;
  }

  .btn-ghost {
    background: transparent;
    color: var(--text-secondary);
  }

  .btn-ghost:hover:not(:disabled) {
    background-color: var(--bg-tertiary);
    color: #e0e0e0;
  }

  .btn-danger {
    background-color: #ff6b6b;
    color: white;
  }

  .btn-danger:hover:not(:disabled) {
    background-color: #ff5252;
  }
</style>
