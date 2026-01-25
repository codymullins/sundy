<script lang="ts">
  import type { HTMLTextareaAttributes } from 'svelte/elements';

  interface Props extends HTMLTextareaAttributes {
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

  const textareaId = id ?? `textarea-${Math.random().toString(36).slice(2)}`;
</script>

<div class="textarea-wrapper {className}">
  {#if label}
    <label for={textareaId} class="textarea-label">{label}</label>
  {/if}
  <textarea 
    id={textareaId}
    class="textarea" 
    class:has-error={!!error}
    bind:value
    {...restProps}
  ></textarea>
  {#if error}
    <span class="textarea-error">{error}</span>
  {/if}
</div>

<style>
  .textarea-wrapper {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .textarea-label {
    font-size: 14px;
    font-weight: 500;
    color: #e0e0e0;
  }

  .textarea {
    width: 100%;
    min-height: 100px;
    padding: 12px 14px;
    background-color: var(--bg-secondary);
    border: 1px solid var(--bg-hover);
    border-radius: 8px;
    color: var(--text-primary);
    font-size: 14px;
    font-family: inherit;
    resize: vertical;
    box-sizing: border-box;
    transition: border-color 0.15s;
    box-shadow: none;
  }

  .textarea:focus {
    outline: none;
    border-color: var(--accent-color);
  }

  .textarea::placeholder {
    color: var(--text-muted);
  }

  .textarea.has-error {
    border-color: #ff6b6b;
  }

  .textarea-error {
    font-size: 12px;
    color: #ff6b6b;
  }
</style>
