#ifndef __COMMON_H__
#define __COMMON_H__

#define NULL 0

inline int min(int a, int b) { return (a < b) ? a : b; }
inline int max(int a, int b) { return (a > b) ? a : b; }

inline double min(double a, double b) { return (a < b) ? a : b; }
inline double max(double a, double b) { return (a > b) ? a : b; }

#endif