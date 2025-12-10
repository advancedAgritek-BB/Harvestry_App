import { Metadata } from 'next';
import Link from 'next/link';
import { Cookie } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Cookie Policy | Harvestry',
  description: 'Learn how Harvestry uses cookies and similar technologies.',
};

export default function CookiePolicyPage() {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="mb-12">
        <div className="flex items-center gap-3 mb-4">
          <Cookie className="w-8 h-8 text-cyan-400" />
          <h1 className="text-3xl sm:text-4xl font-bold text-foreground">
            Cookie Policy
          </h1>
        </div>
        <p className="text-muted-foreground">
          Last updated: December 8, 2025
        </p>
      </div>

      {/* Content */}
      <div className="prose prose-invert prose-lg max-w-none">
        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">1. Introduction</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            This Cookie Policy explains how Harvestry, Inc. (&quot;Harvestry,&quot; &quot;we,&quot; &quot;us,&quot; or &quot;our&quot;) uses cookies 
            and similar tracking technologies when you visit our website at harvestry.io and use our Services.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            By continuing to use our website and Services, you consent to the use of cookies as described in 
            this policy. You can manage your cookie preferences at any time as explained below.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">2. What Are Cookies?</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Cookies are small text files that are placed on your device (computer, smartphone, or tablet) when 
            you visit a website. They are widely used to make websites work more efficiently and to provide 
            information to website owners.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            Cookies can be &quot;persistent&quot; (remaining on your device until they expire or you delete them) or 
            &quot;session&quot; cookies (deleted when you close your browser). They can also be &quot;first-party&quot; (set by 
            us) or &quot;third-party&quot; (set by other companies whose services we use).
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">3. Types of Cookies We Use</h2>

          <h3 className="text-xl font-medium text-foreground mb-3">3.1 Strictly Necessary Cookies</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            These cookies are essential for the website to function properly. They enable core functionality 
            such as security, authentication, and session management. You cannot opt out of these cookies.
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-4 mb-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 text-foreground">Cookie Name</th>
                  <th className="text-left py-2 text-foreground">Purpose</th>
                  <th className="text-left py-2 text-foreground">Duration</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b border-border/50">
                  <td className="py-2">session_id</td>
                  <td className="py-2">Maintains user session</td>
                  <td className="py-2">Session</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">csrf_token</td>
                  <td className="py-2">Security protection</td>
                  <td className="py-2">Session</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">auth_token</td>
                  <td className="py-2">Authentication</td>
                  <td className="py-2">30 days</td>
                </tr>
                <tr>
                  <td className="py-2">cookie_consent</td>
                  <td className="py-2">Stores cookie preferences</td>
                  <td className="py-2">1 year</td>
                </tr>
              </tbody>
            </table>
          </div>

          <h3 className="text-xl font-medium text-foreground mb-3">3.2 Functional Cookies</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            These cookies enable enhanced functionality and personalization, such as remembering your preferences, 
            language settings, and customizations.
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-4 mb-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 text-foreground">Cookie Name</th>
                  <th className="text-left py-2 text-foreground">Purpose</th>
                  <th className="text-left py-2 text-foreground">Duration</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b border-border/50">
                  <td className="py-2">theme_preference</td>
                  <td className="py-2">Dark/light mode setting</td>
                  <td className="py-2">1 year</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">language</td>
                  <td className="py-2">Language preference</td>
                  <td className="py-2">1 year</td>
                </tr>
                <tr>
                  <td className="py-2">dashboard_layout</td>
                  <td className="py-2">Dashboard customization</td>
                  <td className="py-2">1 year</td>
                </tr>
              </tbody>
            </table>
          </div>

          <h3 className="text-xl font-medium text-foreground mb-3">3.3 Analytics Cookies</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            These cookies help us understand how visitors interact with our website by collecting and reporting 
            information anonymously. This helps us improve our Services.
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-4 mb-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 text-foreground">Cookie Name</th>
                  <th className="text-left py-2 text-foreground">Provider</th>
                  <th className="text-left py-2 text-foreground">Purpose</th>
                  <th className="text-left py-2 text-foreground">Duration</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b border-border/50">
                  <td className="py-2">_ga</td>
                  <td className="py-2">Google Analytics</td>
                  <td className="py-2">Distinguishes users</td>
                  <td className="py-2">2 years</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">_ga_*</td>
                  <td className="py-2">Google Analytics</td>
                  <td className="py-2">Maintains session state</td>
                  <td className="py-2">2 years</td>
                </tr>
                <tr>
                  <td className="py-2">_gid</td>
                  <td className="py-2">Google Analytics</td>
                  <td className="py-2">Distinguishes users</td>
                  <td className="py-2">24 hours</td>
                </tr>
              </tbody>
            </table>
          </div>

          <h3 className="text-xl font-medium text-foreground mb-3">3.4 Marketing Cookies</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            These cookies are used to track visitors across websites to display relevant advertisements. They 
            may be set by us or by third-party advertising partners.
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-4 mb-6">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 text-foreground">Cookie Name</th>
                  <th className="text-left py-2 text-foreground">Provider</th>
                  <th className="text-left py-2 text-foreground">Purpose</th>
                  <th className="text-left py-2 text-foreground">Duration</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b border-border/50">
                  <td className="py-2">_fbp</td>
                  <td className="py-2">Facebook</td>
                  <td className="py-2">Ad targeting</td>
                  <td className="py-2">3 months</td>
                </tr>
                <tr>
                  <td className="py-2">li_fat_id</td>
                  <td className="py-2">LinkedIn</td>
                  <td className="py-2">Ad analytics</td>
                  <td className="py-2">30 days</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">4. Similar Technologies</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            In addition to cookies, we may use other similar technologies:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><strong className="text-foreground">Web Beacons:</strong> Small graphic images (also called &quot;pixel tags&quot;) used to track user behavior and conversions.</li>
            <li><strong className="text-foreground">Local Storage:</strong> Technology that allows websites to store data locally on your device, similar to cookies but with larger capacity.</li>
            <li><strong className="text-foreground">Session Storage:</strong> Similar to local storage but data is cleared when the browser session ends.</li>
            <li><strong className="text-foreground">Fingerprinting:</strong> Techniques that collect device characteristics for identification purposes (we minimize use of this technology).</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">5. Managing Your Cookie Preferences</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">5.1 Cookie Consent Banner</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            When you first visit our website, you will see a cookie consent banner that allows you to accept 
            or customize your cookie preferences. You can change these preferences at any time.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">5.2 Browser Settings</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Most web browsers allow you to control cookies through their settings. You can typically:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>View what cookies are stored on your device</li>
            <li>Delete individual or all cookies</li>
            <li>Block cookies from specific or all websites</li>
            <li>Set your browser to notify you when a cookie is set</li>
          </ul>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Here are links to manage cookies in popular browsers:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li><a href="https://support.google.com/chrome/answer/95647" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Google Chrome</a></li>
            <li><a href="https://support.mozilla.org/en-US/kb/cookies-information-websites-store-on-your-computer" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Mozilla Firefox</a></li>
            <li><a href="https://support.apple.com/guide/safari/manage-cookies-sfri11471/mac" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Apple Safari</a></li>
            <li><a href="https://support.microsoft.com/en-us/microsoft-edge/delete-cookies-in-microsoft-edge-63947406-40ac-c3b8-57b9-2a946a29ae09" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Microsoft Edge</a></li>
          </ul>

          <h3 className="text-xl font-medium text-foreground mb-3">5.3 Opt-Out Links</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            You can opt out of certain third-party cookies using these tools:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><a href="https://tools.google.com/dlpage/gaoptout" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Google Analytics Opt-out</a></li>
            <li><a href="https://optout.aboutads.info/" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Digital Advertising Alliance Opt-out</a></li>
            <li><a href="https://www.youronlinechoices.com/" target="_blank" rel="noopener noreferrer" className="text-cyan-400 hover:underline">Your Online Choices (EU)</a></li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">6. Impact of Disabling Cookies</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            If you choose to disable or block cookies, please be aware that:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Some features of our website may not function properly</li>
            <li>You may need to manually adjust settings each time you visit</li>
            <li>You may not be able to access certain parts of the Services</li>
            <li>Your experience may be less personalized</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">7. Do Not Track Signals</h2>
          <p className="text-muted-foreground leading-relaxed">
            Some browsers have a &quot;Do Not Track&quot; (DNT) feature that lets you signal to websites that you do not 
            want to be tracked. Currently, there is no universal standard for responding to DNT signals. Our 
            website does not currently respond to DNT signals, but you can use the cookie management options 
            described above to control tracking.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">8. Changes to This Policy</h2>
          <p className="text-muted-foreground leading-relaxed">
            We may update this Cookie Policy from time to time to reflect changes in technology, legislation, 
            or our data practices. We will post any changes on this page and update the &quot;Last updated&quot; date. 
            We encourage you to review this policy periodically.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">9. Contact Us</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            If you have questions about our use of cookies, please contact us:
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-6">
            <p className="text-foreground font-medium mb-2">Harvestry, Inc.</p>
            <p className="text-muted-foreground">Privacy Team</p>
            <p className="text-muted-foreground">Email: <a href="mailto:privacy@harvestry.io" className="text-cyan-400 hover:underline">privacy@harvestry.io</a></p>
          </div>
        </section>
      </div>

      {/* Related Links */}
      <div className="mt-12 pt-8 border-t border-border">
        <h3 className="text-lg font-semibold text-foreground mb-4">Related Policies</h3>
        <div className="flex flex-wrap gap-4">
          <Link href="/privacy" className="text-cyan-400 hover:underline">Privacy Policy</Link>
          <Link href="/terms" className="text-cyan-400 hover:underline">Terms of Service</Link>
          <Link href="/gdpr" className="text-cyan-400 hover:underline">GDPR Compliance</Link>
        </div>
      </div>
    </div>
  );
}




