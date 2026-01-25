/**
 * Privacy utility functions for masking sensitive data
 */

/**
 * Mask an email address for privacy mode
 * Example: "john.doe@example.com" -> "j***@***.com"
 */
export function maskEmail(email: string): string {
  if (!email || !email.includes('@')) {
    return '***';
  }

  const [localPart, domain] = email.split('@');
  const [domainName, tld] = domain.split('.');

  const maskedLocal = localPart.length > 0 
    ? localPart[0] + '***' 
    : '***';
  
  const maskedDomain = domainName 
    ? '***' 
    : '***';

  return `${maskedLocal}@${maskedDomain}.${tld || '***'}`;
}

/**
 * Get display title for an event based on privacy settings
 */
export function getEventDisplayTitle(
  title: string, 
  privacyMode: boolean, 
  hideEventTitles: boolean
): string {
  if (privacyMode && hideEventTitles) {
    return 'Private Event';
  }
  return title || 'Untitled Event';
}

/**
 * Mask a name for privacy mode
 * Example: "John Doe" -> "J*** D***"
 */
export function maskName(name: string): string {
  if (!name) return '***';
  
  return name.split(' ')
    .map(part => part.length > 0 ? part[0] + '***' : '***')
    .join(' ');
}

/**
 * Get display name for a connected account based on privacy settings
 */
export function getAccountDisplayName(
  displayName: string | null,
  email: string,
  privacyMode: boolean,
  hideEmails: boolean
): string {
  if (privacyMode) {
    if (displayName && !hideEmails) {
      return maskName(displayName);
    }
    return maskEmail(email);
  }
  
  return displayName || email;
}

/**
 * Get display email based on privacy settings
 */
export function getDisplayEmail(
  email: string,
  privacyMode: boolean,
  hideEmails: boolean
): string {
  if (privacyMode && hideEmails) {
    return maskEmail(email);
  }
  return email;
}

/**
 * Mask location for privacy mode
 */
export function maskLocation(location: string): string {
  if (!location) return '';
  
  // Just show first word followed by ***
  const firstWord = location.split(/[\s,]/)[0];
  return firstWord ? `${firstWord}...` : '***';
}

/**
 * Get display location based on privacy settings
 */
export function getDisplayLocation(
  location: string | null,
  privacyMode: boolean
): string {
  if (!location) return '';
  
  if (privacyMode) {
    return maskLocation(location);
  }
  
  return location;
}

/**
 * Mask description for privacy mode
 */
export function getDisplayDescription(
  description: string | null,
  privacyMode: boolean
): string {
  if (!description) return '';
  
  if (privacyMode) {
    return 'Description hidden for privacy';
  }
  
  return description;
}
