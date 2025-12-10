import { Metadata } from 'next';
import Link from 'next/link';
import { Scale, CheckCircle } from 'lucide-react';

export const metadata: Metadata = {
  title: 'GDPR Compliance | Harvestry',
  description: 'Learn about Harvestry\'s commitment to GDPR compliance and data protection.',
};

export default function GDPRPage() {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="mb-12">
        <div className="flex items-center gap-3 mb-4">
          <Scale className="w-8 h-8 text-cyan-400" />
          <h1 className="text-3xl sm:text-4xl font-bold text-foreground">
            GDPR Compliance
          </h1>
        </div>
        <p className="text-muted-foreground">
          Last updated: December 8, 2025
        </p>
      </div>

      {/* Commitment Banner */}
      <div className="bg-emerald-500/10 border border-emerald-500/20 rounded-2xl p-6 mb-12">
        <div className="flex items-start gap-4">
          <CheckCircle className="w-6 h-6 text-emerald-400 flex-shrink-0 mt-1" />
          <div>
            <h2 className="text-lg font-semibold text-emerald-400 mb-2">Our Commitment</h2>
            <p className="text-muted-foreground">
              Harvestry is committed to protecting the privacy and security of personal data in accordance with 
              the General Data Protection Regulation (GDPR) and other applicable data protection laws. We have 
              implemented comprehensive measures to ensure compliance and safeguard your rights.
            </p>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="prose prose-invert prose-lg max-w-none">
        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">1. What is GDPR?</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            The General Data Protection Regulation (GDPR) is a comprehensive data protection law that came into 
            effect on May 25, 2018. It applies to organizations that process personal data of individuals in the 
            European Union (EU) and European Economic Area (EEA), regardless of where the organization is located.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            GDPR establishes strict requirements for how personal data must be collected, processed, stored, and 
            protected, and grants individuals enhanced rights over their personal data.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">2. Our Role Under GDPR</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">2.1 As a Data Controller</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            When Harvestry collects and processes personal data for our own purposes (such as managing customer 
            accounts, marketing, and analytics), we act as a <strong className="text-foreground">Data Controller</strong>. 
            In this role, we determine the purposes and means of processing personal data.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">2.2 As a Data Processor</h3>
          <p className="text-muted-foreground leading-relaxed">
            When we process personal data on behalf of our customers (such as cultivation data and employee information 
            entered into our platform), we act as a <strong className="text-foreground">Data Processor</strong>. 
            We process this data only in accordance with our customers&apos; instructions and applicable agreements.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">3. Lawful Basis for Processing</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We only process personal data when we have a valid lawful basis under GDPR. The lawful bases we rely on include:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><strong className="text-foreground">Contractual Necessity:</strong> Processing necessary to perform our contract with you (e.g., providing the Services you subscribed to).</li>
            <li><strong className="text-foreground">Consent:</strong> Where you have given clear consent for us to process your data for a specific purpose (e.g., marketing communications).</li>
            <li><strong className="text-foreground">Legitimate Interests:</strong> Processing necessary for our legitimate interests, provided they are not overridden by your rights (e.g., improving our Services, security).</li>
            <li><strong className="text-foreground">Legal Obligation:</strong> Processing necessary to comply with legal requirements (e.g., tax records, regulatory reporting).</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">4. Your Rights Under GDPR</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            If you are located in the EU/EEA, you have the following rights regarding your personal data:
          </p>

          <div className="grid gap-4">
            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right of Access (Article 15)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to request a copy of the personal data we hold about you, along with information 
                about how we use it.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Rectification (Article 16)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to request correction of any inaccurate or incomplete personal data we hold about you.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Erasure / &quot;Right to be Forgotten&quot; (Article 17)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to request deletion of your personal data in certain circumstances, such as when 
                it is no longer necessary for the purpose it was collected.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Restriction of Processing (Article 18)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to request that we restrict the processing of your personal data in certain 
                circumstances (e.g., while we verify the accuracy of your data).
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Data Portability (Article 20)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to receive your personal data in a structured, commonly used, machine-readable 
                format and to transmit it to another controller.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Object (Article 21)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right to object to processing based on legitimate interests or for direct marketing purposes.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Rights Related to Automated Decision-Making (Article 22)</h4>
              <p className="text-sm text-muted-foreground">
                You have the right not to be subject to decisions based solely on automated processing that significantly 
                affect you, and to request human intervention.
              </p>
            </div>

            <div className="bg-surface/50 border border-border/50 rounded-xl p-4">
              <h4 className="font-medium text-foreground mb-2">Right to Withdraw Consent</h4>
              <p className="text-sm text-muted-foreground">
                Where we rely on consent to process your data, you have the right to withdraw that consent at any time.
              </p>
            </div>
          </div>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">5. How to Exercise Your Rights</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            To exercise any of your GDPR rights, you can:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>Email us at <a href="mailto:privacy@harvestry.io" className="text-cyan-400 hover:underline">privacy@harvestry.io</a></li>
            <li>Use the data management features in your account settings</li>
            <li>Submit a request through our <Link href="/support" className="text-cyan-400 hover:underline">support portal</Link></li>
          </ul>
          <p className="text-muted-foreground leading-relaxed">
            We will respond to your request within 30 days. We may need to verify your identity before processing 
            your request. In some cases, we may extend this period or charge a reasonable fee, as permitted by GDPR.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">6. International Data Transfers</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            As a company based in the United States, we may transfer personal data from the EU/EEA to the US and 
            other countries. When we do so, we ensure appropriate safeguards are in place:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><strong className="text-foreground">Standard Contractual Clauses (SCCs):</strong> We use EU-approved SCCs for transfers to countries without adequacy decisions.</li>
            <li><strong className="text-foreground">Data Processing Agreements:</strong> We enter into GDPR-compliant DPAs with all sub-processors.</li>
            <li><strong className="text-foreground">Supplementary Measures:</strong> We implement additional technical and organizational measures as needed.</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">7. Data Protection Measures</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We have implemented comprehensive technical and organizational measures to protect personal data, including:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Encryption of data in transit (TLS 1.3) and at rest (AES-256)</li>
            <li>Access controls and role-based permissions</li>
            <li>Regular security assessments and penetration testing</li>
            <li>Employee training on data protection</li>
            <li>Incident response and breach notification procedures</li>
            <li>Data minimization and retention policies</li>
            <li>Privacy by design and default principles</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">8. Data Processing Agreement (DPA)</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            For customers who need a Data Processing Agreement for GDPR compliance, we offer a standard DPA that covers:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>Subject matter and duration of processing</li>
            <li>Nature and purpose of processing</li>
            <li>Types of personal data and categories of data subjects</li>
            <li>Obligations and rights of the controller</li>
            <li>Sub-processor requirements</li>
            <li>Security measures</li>
            <li>Audit rights</li>
            <li>Standard Contractual Clauses</li>
          </ul>
          <p className="text-muted-foreground leading-relaxed">
            To request a DPA, please contact us at <a href="mailto:legal@harvestry.io" className="text-cyan-400 hover:underline">legal@harvestry.io</a>.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">9. Sub-Processors</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            We use trusted sub-processors to help provide our Services. Key sub-processors include:
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-4 mb-4">
            <table className="w-full text-sm">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-left py-2 text-foreground">Sub-Processor</th>
                  <th className="text-left py-2 text-foreground">Purpose</th>
                  <th className="text-left py-2 text-foreground">Location</th>
                </tr>
              </thead>
              <tbody className="text-muted-foreground">
                <tr className="border-b border-border/50">
                  <td className="py-2">Amazon Web Services (AWS)</td>
                  <td className="py-2">Cloud hosting & infrastructure</td>
                  <td className="py-2">US/EU</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">Supabase</td>
                  <td className="py-2">Database & authentication</td>
                  <td className="py-2">US/EU</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">Stripe</td>
                  <td className="py-2">Payment processing</td>
                  <td className="py-2">US</td>
                </tr>
                <tr className="border-b border-border/50">
                  <td className="py-2">SendGrid</td>
                  <td className="py-2">Email delivery</td>
                  <td className="py-2">US</td>
                </tr>
                <tr>
                  <td className="py-2">Google Analytics</td>
                  <td className="py-2">Website analytics</td>
                  <td className="py-2">US</td>
                </tr>
              </tbody>
            </table>
          </div>
          <p className="text-muted-foreground leading-relaxed">
            We maintain an up-to-date list of sub-processors and can notify you of changes upon request.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">10. Data Breach Notification</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            In the event of a personal data breach, we will:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Notify the relevant supervisory authority within 72 hours (where required)</li>
            <li>Notify affected individuals without undue delay if the breach poses a high risk to their rights and freedoms</li>
            <li>Document all breaches, including facts, effects, and remedial actions taken</li>
            <li>Notify customers (as controllers) of any breach affecting their data promptly</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">11. Data Protection Officer</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            For questions about our GDPR compliance or to exercise your rights, you can contact our Privacy Team:
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-6">
            <p className="text-foreground font-medium mb-2">Harvestry Privacy Team</p>
            <p className="text-muted-foreground">Email: <a href="mailto:privacy@harvestry.io" className="text-cyan-400 hover:underline">privacy@harvestry.io</a></p>
          </div>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">12. Complaints</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            If you believe we have not handled your personal data properly, you have the right to lodge a complaint 
            with a supervisory authority. For EU residents, you can contact the data protection authority in your 
            country of residence.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            We encourage you to contact us first at <a href="mailto:privacy@harvestry.io" className="text-cyan-400 hover:underline">privacy@harvestry.io</a> so 
            we can try to resolve your concerns directly.
          </p>
        </section>
      </div>

      {/* Related Links */}
      <div className="mt-12 pt-8 border-t border-border">
        <h3 className="text-lg font-semibold text-foreground mb-4">Related Policies</h3>
        <div className="flex flex-wrap gap-4">
          <Link href="/privacy" className="text-cyan-400 hover:underline">Privacy Policy</Link>
          <Link href="/terms" className="text-cyan-400 hover:underline">Terms of Service</Link>
          <Link href="/cookies" className="text-cyan-400 hover:underline">Cookie Policy</Link>
        </div>
      </div>
    </div>
  );
}




