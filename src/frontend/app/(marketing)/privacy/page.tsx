import { Metadata } from 'next';
import Link from 'next/link';
import { Shield } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Privacy Policy | Harvestry',
  description: 'Learn how Harvestry collects, uses, and protects your personal information.',
};

export default function PrivacyPolicyPage() {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="mb-12">
        <div className="flex items-center gap-3 mb-4">
          <Shield className="w-8 h-8 text-cyan-400" />
          <h1 className="text-3xl sm:text-4xl font-bold text-foreground">
            Privacy Policy
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
            Harvestry, Inc. (&quot;Harvestry,&quot; &quot;we,&quot; &quot;us,&quot; or &quot;our&quot;) is committed to protecting your privacy. 
            This Privacy Policy explains how we collect, use, disclose, and safeguard your information when you 
            use our cultivation operating system platform, website, and related services (collectively, the &quot;Services&quot;).
          </p>
          <p className="text-muted-foreground leading-relaxed">
            Please read this Privacy Policy carefully. By accessing or using our Services, you acknowledge that 
            you have read, understood, and agree to be bound by this Privacy Policy. If you do not agree, please 
            discontinue use of our Services.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">2. Information We Collect</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">2.1 Information You Provide</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We collect information you voluntarily provide when using our Services, including:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-6">
            <li><strong className="text-foreground">Account Information:</strong> Name, email address, phone number, company name, job title, and password when you create an account.</li>
            <li><strong className="text-foreground">Profile Information:</strong> Additional details you add to your profile, such as profile photo, preferences, and settings.</li>
            <li><strong className="text-foreground">Cultivation Data:</strong> Information about your cultivation operations, including crop data, environmental readings, inventory records, compliance documentation, and operational metrics.</li>
            <li><strong className="text-foreground">Payment Information:</strong> Billing address and payment card details (processed securely through our payment processors).</li>
            <li><strong className="text-foreground">Communications:</strong> Messages, feedback, and correspondence when you contact us or participate in surveys.</li>
          </ul>

          <h3 className="text-xl font-medium text-foreground mb-3">2.2 Information Collected Automatically</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            When you access our Services, we automatically collect certain information:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-6">
            <li><strong className="text-foreground">Device Information:</strong> Device type, operating system, unique device identifiers, and browser type.</li>
            <li><strong className="text-foreground">Usage Information:</strong> Pages viewed, features used, time spent, and interaction patterns within our Services.</li>
            <li><strong className="text-foreground">Log Data:</strong> IP address, access times, referring URLs, and system activity logs.</li>
            <li><strong className="text-foreground">Location Information:</strong> General geographic location based on IP address.</li>
            <li><strong className="text-foreground">Cookies and Tracking:</strong> Information collected through cookies, pixels, and similar technologies (see our <Link href="/cookies" className="text-cyan-400 hover:underline">Cookie Policy</Link>).</li>
          </ul>

          <h3 className="text-xl font-medium text-foreground mb-3">2.3 Information from Third Parties</h3>
          <p className="text-muted-foreground leading-relaxed">
            We may receive information from third parties, including integration partners (such as METRC, BioTrack, 
            and other compliance systems), authentication providers, and analytics services.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">3. How We Use Your Information</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We use the information we collect to:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Provide, maintain, and improve our Services</li>
            <li>Process transactions and send related notifications</li>
            <li>Create and manage your account</li>
            <li>Facilitate compliance reporting and regulatory requirements</li>
            <li>Send administrative messages, updates, and security alerts</li>
            <li>Respond to your comments, questions, and support requests</li>
            <li>Analyze usage patterns to improve user experience</li>
            <li>Develop new features and services</li>
            <li>Detect, prevent, and address fraud, security breaches, and technical issues</li>
            <li>Comply with legal obligations</li>
            <li>Send marketing communications (with your consent where required)</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">4. How We Share Your Information</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We do not sell your personal information. We may share your information in the following circumstances:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><strong className="text-foreground">Service Providers:</strong> With trusted vendors who assist in operating our Services (e.g., hosting, analytics, payment processing).</li>
            <li><strong className="text-foreground">Integration Partners:</strong> With third-party services you choose to connect (e.g., compliance tracking systems, accounting software).</li>
            <li><strong className="text-foreground">Regulatory Authorities:</strong> As required for compliance reporting to state and local regulatory bodies.</li>
            <li><strong className="text-foreground">Legal Requirements:</strong> When required by law, regulation, legal process, or governmental request.</li>
            <li><strong className="text-foreground">Business Transfers:</strong> In connection with a merger, acquisition, or sale of assets.</li>
            <li><strong className="text-foreground">With Your Consent:</strong> In other cases where you have given explicit consent.</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">5. Data Security</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We implement robust security measures to protect your information, including:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>Encryption of data in transit (TLS 1.3) and at rest (AES-256)</li>
            <li>Multi-factor authentication options</li>
            <li>Regular security audits and penetration testing</li>
            <li>Access controls and audit logging</li>
            <li>Secure data centers with SOC 2 Type II compliance</li>
            <li>Employee security training and background checks</li>
          </ul>
          <p className="text-muted-foreground leading-relaxed">
            While we strive to protect your information, no method of transmission over the Internet or electronic 
            storage is 100% secure. We cannot guarantee absolute security.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">6. Data Retention</h2>
          <p className="text-muted-foreground leading-relaxed">
            We retain your information for as long as your account is active or as needed to provide Services, 
            comply with legal obligations (including regulatory record-keeping requirements), resolve disputes, 
            and enforce our agreements. Cultivation and compliance data may be retained for longer periods as 
            required by applicable regulations.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">7. Your Rights and Choices</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Depending on your location, you may have the following rights:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li><strong className="text-foreground">Access:</strong> Request a copy of the personal information we hold about you.</li>
            <li><strong className="text-foreground">Correction:</strong> Request correction of inaccurate or incomplete information.</li>
            <li><strong className="text-foreground">Deletion:</strong> Request deletion of your personal information (subject to legal retention requirements).</li>
            <li><strong className="text-foreground">Portability:</strong> Request a portable copy of your data in a machine-readable format.</li>
            <li><strong className="text-foreground">Opt-out:</strong> Opt out of marketing communications at any time.</li>
            <li><strong className="text-foreground">Restriction:</strong> Request restriction of processing in certain circumstances.</li>
          </ul>
          <p className="text-muted-foreground leading-relaxed">
            To exercise these rights, please contact us at <a href="mailto:privacy@harvestry.io" className="text-cyan-400 hover:underline">privacy@harvestry.io</a>.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">8. International Data Transfers</h2>
          <p className="text-muted-foreground leading-relaxed">
            Your information may be transferred to and processed in countries other than your country of residence. 
            These countries may have different data protection laws. When we transfer data internationally, we 
            implement appropriate safeguards such as Standard Contractual Clauses to protect your information.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">9. Children&apos;s Privacy</h2>
          <p className="text-muted-foreground leading-relaxed">
            Our Services are not intended for individuals under 18 years of age. We do not knowingly collect 
            personal information from children. If we learn that we have collected personal information from 
            a child, we will take steps to delete such information.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">10. Third-Party Links</h2>
          <p className="text-muted-foreground leading-relaxed">
            Our Services may contain links to third-party websites and services. We are not responsible for 
            the privacy practices of these third parties. We encourage you to read their privacy policies.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">11. Changes to This Policy</h2>
          <p className="text-muted-foreground leading-relaxed">
            We may update this Privacy Policy from time to time. We will notify you of any material changes 
            by posting the new Privacy Policy on this page and updating the &quot;Last updated&quot; date. For significant 
            changes, we will provide additional notice (such as email notification or in-app announcement).
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">12. Contact Us</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            If you have questions or concerns about this Privacy Policy or our data practices, please contact us:
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
          <Link href="/terms" className="text-cyan-400 hover:underline">Terms of Service</Link>
          <Link href="/cookies" className="text-cyan-400 hover:underline">Cookie Policy</Link>
          <Link href="/gdpr" className="text-cyan-400 hover:underline">GDPR Compliance</Link>
        </div>
      </div>
    </div>
  );
}
