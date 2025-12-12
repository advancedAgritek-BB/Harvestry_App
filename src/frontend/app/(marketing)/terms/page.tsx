import { Metadata } from 'next';
import Link from 'next/link';
import { FileText } from 'lucide-react';

export const metadata: Metadata = {
  title: 'Terms of Service | Harvestry',
  description: 'Terms and conditions for using Harvestry\'s cultivation operating system platform.',
};

export default function TermsOfServicePage() {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="mb-12">
        <div className="flex items-center gap-3 mb-4">
          <FileText className="w-8 h-8 text-cyan-400" />
          <h1 className="text-3xl sm:text-4xl font-bold text-foreground">
            Terms of Service
          </h1>
        </div>
        <p className="text-muted-foreground">
          Last updated: December 8, 2025
        </p>
      </div>

      {/* Content */}
      <div className="prose prose-invert prose-lg max-w-none">
        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">1. Acceptance of Terms</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Welcome to Harvestry. These Terms of Service (&quot;Terms&quot;) constitute a legally binding agreement between 
            you (&quot;User,&quot; &quot;you,&quot; or &quot;your&quot;) and Harvestry, Inc. (&quot;Harvestry,&quot; &quot;we,&quot; &quot;us,&quot; or &quot;our&quot;) governing 
            your access to and use of the Harvestry platform, including our website, applications, APIs, and 
            related services (collectively, the &quot;Services&quot;).
          </p>
          <p className="text-muted-foreground leading-relaxed">
            By creating an account, accessing, or using our Services, you agree to be bound by these Terms. 
            If you are using the Services on behalf of an organization, you represent and warrant that you 
            have the authority to bind that organization to these Terms. If you do not agree to these Terms, 
            you may not use our Services.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">2. Description of Services</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Harvestry provides a cultivation operating system platform designed to help cannabis and agricultural 
            cultivators manage their operations, including but not limited to:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Cultivation management and planning tools</li>
            <li>Environmental monitoring and control integration</li>
            <li>Inventory tracking and management</li>
            <li>Compliance reporting and regulatory integration</li>
            <li>Task and workflow management</li>
            <li>Analytics and reporting dashboards</li>
            <li>Third-party integrations (METRC, BioTrack, etc.)</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">3. Account Registration and Security</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">3.1 Account Creation</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            To use certain features of our Services, you must create an account. You agree to provide accurate, 
            current, and complete information during registration and to keep your account information updated.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">3.2 Account Security</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            You are responsible for maintaining the confidentiality of your account credentials and for all 
            activities that occur under your account. You agree to:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>Use a strong, unique password</li>
            <li>Enable multi-factor authentication when available</li>
            <li>Notify us immediately of any unauthorized access or security breach</li>
            <li>Not share your account credentials with others</li>
          </ul>

          <h3 className="text-xl font-medium text-foreground mb-3">3.3 Account Termination</h3>
          <p className="text-muted-foreground leading-relaxed">
            We reserve the right to suspend or terminate your account if you violate these Terms, engage in 
            fraudulent activity, or for any other reason at our sole discretion with reasonable notice.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">4. Acceptable Use</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            You agree to use our Services only for lawful purposes and in accordance with these Terms. You shall not:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Violate any applicable federal, state, local, or international law or regulation</li>
            <li>Use the Services in connection with any unlawful cultivation or distribution activities</li>
            <li>Submit false, misleading, or fraudulent information</li>
            <li>Interfere with or disrupt the Services or servers</li>
            <li>Attempt to gain unauthorized access to any part of the Services</li>
            <li>Use automated systems or software to extract data from the Services (scraping)</li>
            <li>Reverse engineer, decompile, or disassemble any part of the Services</li>
            <li>Use the Services to transmit malware, viruses, or other harmful code</li>
            <li>Impersonate any person or entity</li>
            <li>Harass, abuse, or harm other users</li>
            <li>Use the Services to send spam or unsolicited communications</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">5. Subscription and Payment</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">5.1 Subscription Plans</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Access to certain features requires a paid subscription. Current pricing and plan details are 
            available on our website. We reserve the right to modify pricing with 30 days&apos; notice.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">5.2 Billing</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Subscriptions are billed in advance on a monthly or annual basis. By subscribing, you authorize 
            us to charge your payment method for recurring fees until you cancel.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">5.3 Cancellation and Refunds</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            You may cancel your subscription at any time. Cancellation takes effect at the end of the current 
            billing period. No refunds are provided for partial periods, except as required by law or stated 
            in our refund policy.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">5.4 Taxes</h3>
          <p className="text-muted-foreground leading-relaxed">
            Prices do not include applicable taxes. You are responsible for all taxes associated with your 
            subscription, except for taxes based on our net income.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">6. Intellectual Property</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">6.1 Our Intellectual Property</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            The Services and all content, features, and functionality (including but not limited to software, 
            text, graphics, logos, and user interface design) are owned by Harvestry or its licensors and are 
            protected by intellectual property laws. You receive a limited, non-exclusive, non-transferable 
            license to use the Services according to these Terms.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">6.2 Your Content</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            You retain ownership of the data and content you submit to the Services (&quot;Your Content&quot;). By 
            submitting Your Content, you grant us a worldwide, royalty-free license to use, store, process, 
            and display Your Content solely to provide and improve the Services.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">6.3 Feedback</h3>
          <p className="text-muted-foreground leading-relaxed">
            Any feedback, suggestions, or ideas you provide about our Services may be used by us without 
            obligation or compensation to you.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">7. Data and Privacy</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Your use of the Services is also governed by our <Link href="/privacy" className="text-cyan-400 hover:underline">Privacy Policy</Link>, 
            which describes how we collect, use, and protect your information. By using the Services, you 
            consent to our data practices as described in the Privacy Policy.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            You are responsible for ensuring that your use of the Services complies with all applicable 
            data protection and privacy laws, including obtaining any necessary consents from individuals 
            whose data you input into the Services.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">8. Compliance and Regulatory</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            While Harvestry provides tools to assist with regulatory compliance, you are solely responsible 
            for ensuring your operations comply with all applicable laws and regulations. Our Services are 
            designed to facilitate compliance but do not guarantee regulatory approval or protection from 
            enforcement actions.
          </p>
          <p className="text-muted-foreground leading-relaxed">
            You acknowledge that cannabis cultivation may be subject to complex and varying regulations at 
            federal, state, and local levels. You agree to use our Services only in jurisdictions where 
            your activities are legally permitted.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">9. Third-Party Integrations</h2>
          <p className="text-muted-foreground leading-relaxed">
            Our Services may integrate with third-party services (such as METRC, BioTrack, and other platforms). 
            Your use of third-party services is subject to their respective terms and policies. We are not 
            responsible for the availability, accuracy, or conduct of third-party services.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">10. Disclaimer of Warranties</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            THE SERVICES ARE PROVIDED &quot;AS IS&quot; AND &quot;AS AVAILABLE&quot; WITHOUT WARRANTIES OF ANY KIND, EXPRESS OR 
            IMPLIED. TO THE FULLEST EXTENT PERMITTED BY LAW, WE DISCLAIM ALL WARRANTIES, INCLUDING:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li>Warranties of merchantability, fitness for a particular purpose, and non-infringement</li>
            <li>Warranties that the Services will be uninterrupted, error-free, or secure</li>
            <li>Warranties regarding the accuracy or completeness of any information</li>
            <li>Warranties that the Services will meet your requirements</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">11. Limitation of Liability</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            TO THE MAXIMUM EXTENT PERMITTED BY LAW, HARVESTRY AND ITS OFFICERS, DIRECTORS, EMPLOYEES, AND 
            AGENTS SHALL NOT BE LIABLE FOR ANY INDIRECT, INCIDENTAL, SPECIAL, CONSEQUENTIAL, OR PUNITIVE 
            DAMAGES, INCLUDING BUT NOT LIMITED TO:
          </p>
          <ul className="list-disc list-inside text-muted-foreground space-y-2 mb-4">
            <li>Loss of profits, revenue, or data</li>
            <li>Business interruption</li>
            <li>Regulatory fines or penalties</li>
            <li>Crop loss or damage</li>
            <li>Any damages arising from your use or inability to use the Services</li>
          </ul>
          <p className="text-muted-foreground leading-relaxed">
            OUR TOTAL LIABILITY SHALL NOT EXCEED THE AMOUNT YOU PAID TO US IN THE TWELVE (12) MONTHS PRECEDING 
            THE CLAIM, OR $100 IF YOU HAVE NOT MADE ANY PAYMENTS.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">12. Indemnification</h2>
          <p className="text-muted-foreground leading-relaxed">
            You agree to indemnify, defend, and hold harmless Harvestry and its officers, directors, employees, 
            agents, and affiliates from any claims, liabilities, damages, losses, costs, or expenses (including 
            reasonable attorneys&apos; fees) arising from your use of the Services, violation of these Terms, or 
            infringement of any third-party rights.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">13. Dispute Resolution</h2>
          
          <h3 className="text-xl font-medium text-foreground mb-3">13.1 Governing Law</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            These Terms shall be governed by and construed in accordance with the laws of the State of Delaware, 
            without regard to its conflict of law provisions.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">13.2 Arbitration</h3>
          <p className="text-muted-foreground leading-relaxed mb-4">
            Any dispute arising from these Terms shall be resolved through binding arbitration administered by 
            the American Arbitration Association in accordance with its Commercial Arbitration Rules. The 
            arbitration shall take place in Delaware or remotely at the parties&apos; mutual agreement.
          </p>

          <h3 className="text-xl font-medium text-foreground mb-3">13.3 Class Action Waiver</h3>
          <p className="text-muted-foreground leading-relaxed">
            You agree to resolve disputes with us on an individual basis and waive any right to participate in 
            a class action lawsuit or class-wide arbitration.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">14. Changes to Terms</h2>
          <p className="text-muted-foreground leading-relaxed">
            We may modify these Terms at any time. We will provide notice of material changes by posting the 
            updated Terms on our website and updating the &quot;Last updated&quot; date. Your continued use of the 
            Services after changes become effective constitutes your acceptance of the revised Terms.
          </p>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">15. General Provisions</h2>
          <ul className="list-disc list-inside text-muted-foreground space-y-2">
            <li><strong className="text-foreground">Entire Agreement:</strong> These Terms constitute the entire agreement between you and Harvestry regarding the Services.</li>
            <li><strong className="text-foreground">Severability:</strong> If any provision is found unenforceable, the remaining provisions shall continue in effect.</li>
            <li><strong className="text-foreground">Waiver:</strong> Our failure to enforce any right or provision shall not constitute a waiver.</li>
            <li><strong className="text-foreground">Assignment:</strong> You may not assign these Terms without our prior written consent.</li>
            <li><strong className="text-foreground">Force Majeure:</strong> We are not liable for delays or failures due to circumstances beyond our reasonable control.</li>
          </ul>
        </section>

        <section className="mb-10">
          <h2 className="text-2xl font-semibold text-foreground mb-4">16. Contact Information</h2>
          <p className="text-muted-foreground leading-relaxed mb-4">
            For questions about these Terms, please contact us:
          </p>
          <div className="bg-surface/50 border border-border/50 rounded-xl p-6">
            <p className="text-foreground font-medium mb-2">Harvestry, Inc.</p>
            <p className="text-muted-foreground">Legal Department</p>
            <p className="text-muted-foreground">Email: <a href="mailto:legal@harvestry.io" className="text-cyan-400 hover:underline">legal@harvestry.io</a></p>
          </div>
        </section>
      </div>

      {/* Related Links */}
      <div className="mt-12 pt-8 border-t border-border">
        <h3 className="text-lg font-semibold text-foreground mb-4">Related Policies</h3>
        <div className="flex flex-wrap gap-4">
          <Link href="/privacy" className="text-cyan-400 hover:underline">Privacy Policy</Link>
          <Link href="/cookies" className="text-cyan-400 hover:underline">Cookie Policy</Link>
          <Link href="/gdpr" className="text-cyan-400 hover:underline">GDPR Compliance</Link>
        </div>
      </div>
    </div>
  );
}





