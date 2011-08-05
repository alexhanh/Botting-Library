/*
  Random number generator class
  =============================
  History:
 
  Created - Sarah "Voodoo Doll" White (2006/01/24)
  =============================
  Description:
 
  This class wraps the Mersenne Twister generator
  with a public interface that supports three common
  pseudorandom number requests:
 
  === Uniform deviate [0,1) ===
  Random rnd(seed);
  double r = rnd.uniform();
 
  === Uniform deviate [0,hi) ===
  Random rnd(seed);
  unsigned long r = rnd.uniform(hi);
 
  === Uniform deviate [lo,hi) ===
  Random rnd(seed);
  unsigned long r = rnd.uniform(lo, hi);
 
  seed, lo, and hi are user supplied values, with
  seed having a default setting of 1 for debugging
  and testing purposes.
*/

#ifndef __RANDOM_H__
#define __RANDOM_H__

class Random {
  // Arbitrary constants that work well
  static const int           N = 624;
  static const int           M = 397;
  static const unsigned long MATRIX_A = 0x9908b0dfUL;
  static const unsigned long UPPER_MASK = 0x80000000UL;
  static const unsigned long LOWER_MASK = 0x7fffffffUL;
  static const unsigned long MAX = 0xffffffffUL;
 
  unsigned long x[N]; // Random number pool
  int           next; // Current pool index
public:
  Random(unsigned long seed = 1) : next(0) { seedgen(seed); }
 
  // Return a uniform deviate in the range [0,1)
  double uniform();
  // Return a uniform deviate in the range [0,hi)
  unsigned uniform(unsigned hi);
  // Return a uniform deviate in the range [lo,hi)
  unsigned uniform(unsigned lo, unsigned hi);
private:
  void seedgen(unsigned long seed);
  unsigned long randgen();
};
 
#endif