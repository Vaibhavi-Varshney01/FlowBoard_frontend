import { SignIn } from '@clerk/clerk-react';

// Dedicated page for Clerk's hosted sign-in (handles Google/GitHub OAuth)
const ClerkSignIn = () => {
  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      minHeight: '80vh'
    }}>
      <SignIn
        routing="path"
        path="/sign-in"
        fallbackRedirectUrl="/sso-callback"
        signUpUrl="/register"
      />
    </div>
  );
};

export default ClerkSignIn;
