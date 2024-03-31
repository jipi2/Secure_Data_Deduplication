# A proposal for data deduplication on end-to-end encrypted documents


## This project presents a solution for secure storage with the possibility of deduplication

To obtain more efficient solutions for data transfer and storage, data deduplication techniques were designed and implemented. Still, it was proved that those first schemes,
implemented by large companies, such as Dropbox, were
insecure and allowed for unauthorized data access. Since
then, many schemes for secure data deduplication have been
proposed.

From 2010 to the present day,
security properties for data deduplication were tackled mainly
using the following approaches:
- Proof of ownership .
- Message-dependent encryption.
- Traffic Obfuscation
- Deterministic information dispersal 

Alongside the above mentioned approaches, semantic secure
data deduplication schemes were proposed, but were
proven to be unusable from a user-experience perspective.
Most solutions to approach deduplication problem via Proof
of ownership are based on Merkle Hash Tree, which are
a good method to generate and check challenges regarding
data content. Although MHT seem to be a sensible approach
for this solution, MHT are I/O intensive, therefore are not as
efficient as one might wish. In our approach, the server stores a
set of precomputed challenges that are to be used when another
user would claim the ownership of the same document.

Message-dependant encryption started using a simple aproach to encrypt documents using a some information generated from the file content itself. This way, one may assure a
better deduplication factor, since two identical cleartext have
the same cyphertext. This proposals evolved over time by using
key servers and they also could generate a randomized tag, that
is used to identify the document.

To enhance data deduplication efficiency and security, we
propose using a proxy server as an intermediary between the
client and server. This approach expedites Proof of Ownership (PoW) computation, reducing client-side workload and
mitigating security risks from network monitoring attacks. By
acting as a protective barrier, the proxy server enhances overall
performance and security in the deduplication process.

## System diagram

![System_diagram](./img/System_Diagram.png)

## Repo Structure

The repository comprises two folders. The 'web_app' directory contains a project featuring a minimal web interface. This project loads files into memory, which means it's limited by the file size. The second folder, 'desktop_app,' contains a project built as a desktop application, tailored for handling large files. This one is also minimal in nature.