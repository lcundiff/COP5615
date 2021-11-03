We implemented the bonus where a node dies. 

For the purposes of this assignment, a node dies after sending X number of requests. When it dies, the other nodes do not have knowledge
of its death. The finger tables however are smarter in our example.  On the death of a node, the keys of that node are moved over to the
succeeding node. The finger tables are smarter in the sense that they stay up to date regardless of the information that they are given. That is, we fix our finger tables accordingly.

We tested this in numerous ways where a node dies at the beginning, a node dies midway through (current implementation), and a node dies after it completes sending all of its requests. 

We found that the because our finger tables are always staying up to date, the average hops and run time is still similar (estimation). The performance does not take a major hit as the adding of a node is very similar to the removal of the node.  Nodes need to learn that another dies and update their understanding of their finger table, which we have done so.

- Chris & Logan